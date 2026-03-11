// New file: minimal ICategoryRepository
using System;
using System.Collections.Generic;
using MicroServiceProduct.Domain.Models;

namespace MicroServiceProduct.Domain.Interfaces
{
    public interface ICategoryRepository
    {
        void Create(Category entity);
        Category? Read(Guid id);
        void Update(Category entity);
        void Delete(Guid id);
        List<Category> GetAll();
    }
}
