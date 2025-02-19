﻿using AutoMapper;
using Emerce_DB;
using Emerce_Extension;
using Emerce_Model;
using Emerce_Model.Product;
using Emerce_Service.Validator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emerce_Service.Product
{
    public class ProductService : IsValidBase, IProductService
    {
        private readonly IMapper mapper;

        public ProductService( IMapper _mapper )
        {
            mapper = _mapper;
        }

        //Create Product (uses data annotations to validate data)
        public General<ProductViewModel> Insert( ProductCreateModel newProduct )
        {
            var result = new General<ProductViewModel>();
            var model = mapper.Map<Emerce_DB.Entities.Product>(newProduct);
            using ( var service = new EmerceContext() )
            {

                if ( !IsValidUser(service, model.Iuser) )
                {
                    result.ExceptionMessage = $"User with id:{model.Iuser} is not found";
                    return result;
                }
                if ( !IsValidCategory(service, model.CategoryId) )
                {
                    result.ExceptionMessage = $"Category with id:{model.CategoryId} is not found";
                    return result;
                }
                model.Idatetime = DateTime.Now;
                model.IsActive = true;
                service.Product.Add(model);
                service.SaveChanges();
                result.Entity = mapper.Map<ProductViewModel>(model);
                result.IsSuccess = true;
            }
            return result;
        }
        //Get Product List
        //public General<ProductViewModel> Get()
        //{
        //    var result = new General<ProductViewModel>();
        //    using ( var service = new EmerceContext() )
        //    {
        //        var data = service.Product.Include(p => p.Category)
        //            .Where(p => p.IsActive && !p.IsDeleted)
        //            .Include(p => p.IuserNavigation)
        //            .OrderBy(p => p.Id);

        //        result.List = mapper.Map<List<ProductViewModel>>(data);

        //        //Using extension ToTurkishLira here. Takes ProductViewModel item as input and converts price to Turkish Lira type.
        //        //foreach ( var item in result.List )
        //        //{
        //        //    item.ToTurkishLira();
        //        //}
        //        result.IsSuccess = true;
        //        result.TotalCount = data.Count();
        //    }
        //    return result;
        //}

        //Get Products - takes pageSize and pageNumber inputs, max pageSize is 25.
        public General<ProductViewModel> Get( int pageNumber, int pageSize )
        {
            var result = new General<ProductViewModel>();
            using ( var service = new EmerceContext() )
            {
                var data = service.Product
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && !p.IsDeleted)
                    .Include(p => p.IuserNavigation)
                    .Skip(( pageNumber - 1 ) * pageSize)
                    .Take(pageSize)
                    .OrderBy(p => p.Id);
                result.List = mapper.Map<List<ProductViewModel>>(data);
                result.IsSuccess = true;
                result.TotalCount = service.Product.Where(p => p.IsActive && !p.IsDeleted).Count();
            }
            return result;
        }
        //Get Products - Sorted 
        public General<ProductViewModel> GetSorted( string sorting )
        {
            var result = new General<ProductViewModel>();
            using ( var service = new EmerceContext() )
            {
                var data = service.Product
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && !p.IsDeleted)
                    .Include(p => p.IuserNavigation)
                    .OrderBy(sorting, false);
                result.List = mapper.Map<List<ProductViewModel>>(data);
                result.IsSuccess = true;
                result.TotalCount = service.Product.Where(p => p.IsActive && !p.IsDeleted).Count();
            }

            return result;
        }

        //Get Product - Paging + Sorting 
        public General<ProductViewModel> GetPagesSorted( int pageNumber, int pageSize, string sorting )
        {
            var result = new General<ProductViewModel>();
            using ( var service = new EmerceContext() )
            {
                var data = service.Product
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && !p.IsDeleted)
                    .Include(p => p.IuserNavigation)
                    .OrderBy(sorting, false)//false = ascending , true = descending
                    .Skip(( pageNumber - 1 ) * pageSize)
                    .Take(pageSize);
                result.List = mapper.Map<List<ProductViewModel>>(data);
                result.IsSuccess = true;
                result.TotalCount = service.Product.Where(p => p.IsActive && !p.IsDeleted).Count();
            }
            return result;
        }

        //Get Product - Filtering
        public General<ProductViewModel> GetFiltered( int minPrice, int maxPrice )
        {
            var result = new General<ProductViewModel>();
            using ( var service = new EmerceContext() )
            {
                var data = service.Product
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && !p.IsDeleted && p.Price >= minPrice && p.Price <= maxPrice)
                    .Include(p => p.IuserNavigation)
                    .OrderBy(p => p.Price);

                result.List = mapper.Map<List<ProductViewModel>>(data);
                result.IsSuccess = true;
                result.TotalCount = service.Product.Where(p => p.IsActive && !p.IsDeleted).Count();
            }
            return result;

        }



        //Get Product By Id
        public General<ProductViewModel> GetById( int id )
        {
            var result = new General<ProductViewModel>();
            using ( var service = new EmerceContext() )
            {
                var data = service.Product.Include(p => p.Category)
                    .Include(p => p.IuserNavigation)
                    .SingleOrDefault(p => p.Id == id && p.IsActive && !p.IsDeleted);
                if ( data is null )
                {
                    result.ExceptionMessage = $"Product with id:{id} is not found";
                    return result;
                }
                result.Entity = mapper.Map<ProductViewModel>(data);
                result.IsSuccess = true;
            }
            return result;
        }
        //Update Product
        public General<ProductUpdateModel> Update( ProductUpdateModel updatedProduct, int id )
        {
            var result = new General<ProductUpdateModel>();

            using ( var service = new EmerceContext() )
            {
                var data = service.Product.SingleOrDefault(p => p.Id == id);
                if ( data is null )
                {
                    result.ExceptionMessage = $"Product with id:{id} is not found";
                    return result;
                }
                if ( !IsValidUser(service, ( int )updatedProduct.Uuser) )
                {
                    result.ExceptionMessage = $"User with id:{updatedProduct.Uuser} is not found";
                    return result;
                }

                if ( !IsValidCategory(service, updatedProduct.CategoryId) )
                {
                    result.ExceptionMessage = $"Category with id:{updatedProduct.CategoryId} is not found";
                    return result;
                }
                data.Udatetime = DateTime.Now;
                data.CategoryId = updatedProduct.CategoryId != default ? updatedProduct.CategoryId : data.CategoryId;
                data.Price = updatedProduct.Price != default ? updatedProduct.Price : data.Price;
                data.Stock = updatedProduct.Stock != default ? updatedProduct.Stock : data.Stock;
                data.Description = String.IsNullOrEmpty(updatedProduct.Description.Trim()) ? data.Description : updatedProduct.Description;
                data.Name = String.IsNullOrEmpty(updatedProduct.Name.Trim()) ? data.Name : updatedProduct.Name;
                data.DisplayName = String.IsNullOrEmpty(updatedProduct.DisplayName.Trim()) ? data.DisplayName : updatedProduct.DisplayName;
                service.SaveChanges();
                result.Entity = mapper.Map<ProductUpdateModel>(updatedProduct);
                result.IsSuccess = true;
            }
            return result;
        }

        //Delete Product
        public General<ProductViewModel> Delete( int id )
        {
            var result = new General<ProductViewModel>();

            using ( var service = new EmerceContext() )
            {
                var data = service.Product.SingleOrDefault(p => p.Id == id);
                if ( data is null || data.IsDeleted )
                {
                    result.ExceptionMessage = $"Product with id: {id} is not found";
                    return result;
                }
                //service.Product.Remove(data);
                data.IsDeleted = true;
                data.IsActive = false;
                service.SaveChanges();
                result.Entity = mapper.Map<ProductViewModel>(data);

                result.IsSuccess = true;
            }

            return result;
        }


    }
}
