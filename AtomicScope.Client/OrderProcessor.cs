using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AtomicScope.Client
{
    public class OrderProcessor
    {
        private List<OrderItem> _cartItems;
        private readonly AsActivityLogger _asActivityLogger;
        private string _address;
        private PaymentOption _paymentOption;

        public OrderProcessor()
        {
            var apiResourceId = new Guid("553b8d9c-d8cc-41d0-ac2d-237b68881f77");
            _asActivityLogger = new AsActivityLogger(apiResourceId, "Order Processing", "Purchase Order");
            _cartItems = new List<OrderItem>();
        }

        public void AddItemToCart(List<OrderItem> items)
        {
            _asActivityLogger.Start("Add Item To Cart");
            _cartItems = items.ToList();
            var trackedProperties = new Dictionary<string, string>();
            int itemCount = 1;
            foreach (var item in _cartItems)
            {
                trackedProperties.Add($"Item {itemCount++}", item.Name);
            }

            _asActivityLogger.Update(trackedProperties);
        }

        public void AddAddress(string address)
        {
            _asActivityLogger.Start("Add Address");
            _address = address;
            _asActivityLogger.Update(new Dictionary<string, string>
            {
                {"address",address}
            });
        }

        public void SelectPaymentOption(PaymentOption paymentOption)
        {
            _paymentOption = paymentOption;
            string items = string.Join(",", _cartItems.Select(s => s.Name));
            _asActivityLogger.Log("Checkout", new Dictionary<string, string>
            {
                {"Name", "Tom"},
                {"Address", _address},
                {"Items",items },
                {"Payment",_paymentOption.ToString()}
            });
        }

        public void Pay()
        {
            Console.WriteLine($"Your order item(s) { string.Join(",", _cartItems.Select(s => s.Name))} has been placed.");
            Console.WriteLine($"To Address : {_address}");
            Console.WriteLine($"Payment Option : {_paymentOption}");
            var archiveMessage = new Dictionary<string, string>
            {
                {"Ordered Items",string.Join(",", _cartItems.Select(s => s.Name))},
                {"To Address ",_address},
                {"Payment Option",_paymentOption.ToString()}
            };
            _asActivityLogger.Archive(JsonConvert.SerializeObject(archiveMessage));
        }
    }
    public class OrderItem
    {
        public string Name { get; set; }
    }

    public enum PaymentOption
    {
        CashOnDelivery,
        OnlineBanking
    }

}

