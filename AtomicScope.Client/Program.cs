using System;
using System.Collections.Generic;

namespace AtomicScope.Client
{
    public class Program
    {
        static void Main()
        {
            var orderProcessor = new OrderProcessor();
            List<OrderItem> newOrder = new List<OrderItem>();
            do
            {
                Console.WriteLine("Enter the item to buy:");
                var itemName = Console.ReadLine();

                newOrder.Add(new OrderItem
                {
                    Name = itemName
                });
                Console.WriteLine("Press 1 to proceed with Checkout. Press 2 to continue adding items.");

                var userInput = Console.ReadLine();
                if (userInput == "1")
                {
                    orderProcessor.AddItemToCart(newOrder);
                    break;
                }
            } while (true);

            //Add Address
            Console.WriteLine("Please enter your address");
            var address = Console.ReadLine();
            orderProcessor.AddAddress(address);

            //Select Payment method
            Console.WriteLine("Select payment:");
            Console.WriteLine("1. Cash on delivery");
            Console.WriteLine("2. Online banking");
            var paymentPreference = Console.ReadLine();
            orderProcessor.SelectPaymentOption(paymentPreference == "1"
                ? PaymentOption.CashOnDelivery
                : PaymentOption.OnlineBanking);

            //Place order
            Console.WriteLine("Do you want to proceed to pay? (Y/N)");
            var paymentConfirmation = Console.ReadLine();
            if (paymentConfirmation != null && paymentConfirmation.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                orderProcessor.Pay();
            else
                Console.WriteLine("You order has been cancelled.");

            Console.ReadKey();
        }
    }
}
