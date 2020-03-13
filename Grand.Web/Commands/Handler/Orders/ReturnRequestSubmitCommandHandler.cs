﻿using Grand.Core;
using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Orders;
using Grand.Services.Catalog;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Services.Orders;
using Grand.Web.Commands.Models.Orders;
using Grand.Web.Models.Orders;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Orders
{
    public class ReturnRequestSubmitCommandHandler : IRequestHandler<ReturnRequestSubmitCommandModel, (ReturnRequestModel model, ReturnRequest rr)>
    {
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IProductService _productService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ILocalizationService _localizationService;
        private readonly LocalizationSettings _localizationSettings;

        
        public ReturnRequestSubmitCommandHandler(IWorkContext workContext,
            IStoreContext storeContext,
            IProductService productService,
            IReturnRequestService returnRequestService,
            IWorkflowMessageService workflowMessageService,
            ILocalizationService localizationService,
            LocalizationSettings localizationSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _productService = productService;
            _returnRequestService = returnRequestService;
            _workflowMessageService = workflowMessageService;
            _localizationService = localizationService;
            _localizationSettings = localizationSettings;
        }

        public async Task<(ReturnRequestModel model, ReturnRequest rr)> Handle(ReturnRequestSubmitCommandModel request, CancellationToken cancellationToken)
        {
            var rr = new ReturnRequest {
                StoreId = _storeContext.CurrentStore.Id,
                OrderId = request.Order.Id,
                CustomerId = _workContext.CurrentCustomer.Id,
                CustomerComments = request.Model.Comments,
                StaffNotes = string.Empty,
                ReturnRequestStatus = ReturnRequestStatus.Pending,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                PickupAddress = request.Address,
            };

            if (request.Model.PickupDate.HasValue)
                rr.PickupDate = request.Model.PickupDate.Value;

            foreach (var orderItem in request.Order.OrderItems)
            {
                var product = await _productService.GetProductById(orderItem.ProductId);
                if (!product.NotReturnable)
                {
                    int quantity = 0; //parse quantity
                    string rrrId = "";
                    string rraId = "";

                    foreach (string formKey in request.Form.Keys)
                    {
                        if (formKey.Equals(string.Format("quantity{0}", orderItem.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            int.TryParse(request.Form[formKey], out quantity);
                        }

                        if (formKey.Equals(string.Format("reason{0}", orderItem.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            rrrId = request.Form[formKey];
                        }

                        if (formKey.Equals(string.Format("action{0}", orderItem.Id), StringComparison.OrdinalIgnoreCase))
                        {
                            rraId = request.Form[formKey];
                        }
                    }

                    if (quantity > 0)
                    {
                        var rrr = await _returnRequestService.GetReturnRequestReasonById(rrrId);
                        var rra = await _returnRequestService.GetReturnRequestActionById(rraId);
                        rr.ReturnRequestItems.Add(new ReturnRequestItem {
                            RequestedAction = rra != null ? rra.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id) : "not available",
                            ReasonForReturn = rrr != null ? rrr.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id) : "not available",
                            Quantity = quantity,
                            OrderItemId = orderItem.Id
                        });
                    }
                }
            }

            if (rr.ReturnRequestItems.Any())
            {
                await _returnRequestService.InsertReturnRequest(rr);

                //notify store owner here (email)
                await _workflowMessageService.SendNewReturnRequestStoreOwnerNotification(rr, request.Order, _localizationSettings.DefaultAdminLanguageId);
                //notify customer
                await _workflowMessageService.SendNewReturnRequestCustomerNotification(rr, request.Order, request.Order.CustomerLanguageId);
            }
            else
            {
                request.Model.Error = _localizationService.GetResource("ReturnRequests.NoItemsSubmitted");
            }

            return (request.Model, rr);
        }
    }
}