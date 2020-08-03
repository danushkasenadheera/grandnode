﻿using Grand.Api.DTOs.Catalog;
using MediatR;

namespace Grand.Api.Commands.Models.Catalog
{
    public class UpdateProductSpecificationCommand : IRequest<bool>
    {
        public ProductDto Product { get; set; }
        public ProductSpecificationAttributeDto Model { get; set; }
    }
}
