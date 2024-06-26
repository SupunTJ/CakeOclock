﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Tangy_Business.Repository.IRepository;
using Tangy_Models;

namespace TangyWeb_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEmailSender _emailSender;

        public OrderController(IOrderRepository orderRepository, IEmailSender emailSender)
        {
            _orderRepository = orderRepository;
            _emailSender = emailSender;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _orderRepository.GetAll());
        }

        [HttpGet("{orderHeaderId}")]
        public async Task<IActionResult> Get(int? orderHeaderId)
        {
            if (orderHeaderId == null || orderHeaderId == 0)
            {
                return BadRequest(new ErrorModelDTO()
                {
                    ErrorMessage = "Invalid Id",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            var orderHeader = await _orderRepository.Get(orderHeaderId.Value);
            if (orderHeader == null)
            {
                return BadRequest(new ErrorModelDTO()
                {
                    ErrorMessage = "Invalid Id",
                    StatusCode = StatusCodes.Status404NotFound
                });
            }

            return Ok(orderHeader);
        }

        

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> Create([FromBody] OrderDTO orderDTO)
        {
            if (orderDTO == null || orderDTO.OrderHeader == null)
            {
                return BadRequest(new ErrorModelDTO()
                {
                    ErrorMessage = "OrderDTO or OrderHeader is null",
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }

            // Assuming that the orderDTO is already validated and contains necessary information
            if (orderDTO.OrderHeader != null)
            {
                orderDTO.OrderHeader.OrderDate = DateTime.Now;
            }

            var result = await _orderRepository.Create(orderDTO);
            var ownerEmail = "cakeoclockbakery123@gmail.com";

            if (result != null && result.OrderHeader != null) // Ensure result and result.OrderHeader are not null
            {
                // HTML content for customer email
                string customerEmailContent = @"
            <html>
            <head>
                <style>
                    /* CSS styles for email */
                    body {
                        font-family: Arial, sans-serif;
                        background-color: #f0f0f0;
                        padding: 20px;
                    }
                    .order-summary {
                        background-color: #ffffff;
                        padding: 15px;
                        border-radius: 5px;
                        box-shadow: 0 0 10px rgba(0,0,0,0.1);
                        margin-bottom: 20px;
                        border: 2px solid #ff6600; /* Add border */
                        overflow: auto; /* Clear floats */
                    }
                    .order-summary h2 {
                        color: #ff6600;
                    }
                    .order-summary p {
                        color: #333333;
                        margin: 5px 0; /* Add spacing between paragraphs */
                        clear: both; /* Ensure each paragraph starts below any floated elements */
                    }
                    .order-summary img {
                        float: left; /* Float image to the left */
                        margin-right: 20px; /* Space between image and text */
                        margin-bottom: 10px; /* Space between image and surrounding content */
                        max-width: 100px; /* Adjust as needed */
                        height: auto; /* Maintain aspect ratio */
                    }
                    .order-summary p strong {
                        display: inline-block;
                        width: 180px; /* Adjust width for label alignment */
                    }
                    .order-details {
                        display: inline-block;
                        vertical-align: top;
                    }
                </style>
            </head>
           <body>
                <div class='order-summary'>
                    <img src='https://reactdotnetimages.blob.core.windows.net/reactdotnet/cake-logo.jpg' alt='Order Image'>
                    <h2>New Order Confirmation</h2>
                    <p><strong>Order Id:</strong> <span class='order-details'>" + result.OrderHeader.Id + @"</span></p>
                    <p><strong>Customer Name:</strong> <span class='order-details'>" + result.OrderHeader.Name + @"</span></p>
                    <p><strong>Customer Contact Number:</strong> <span class='order-details'>" + result.OrderHeader.PhoneNumber + @"</span></p>
                    <p><strong>Customer Address:</strong> <span class='order-details'>" + result.OrderHeader.StreetAddress + @"</span></p>
                    <p><strong>Customer Postal Code:</strong> <span class='order-details'>" + result.OrderHeader.PostalCode + @"</span></p>
                    <p><strong>Order Date:</strong> <span class='order-details'>" + result.OrderHeader.OrderDate + @"</span></p>
                    <h3><strong>Bill Amount:</strong> Rs. <span class='order-details'>" + result.OrderHeader.OrderTotal + @"</span></h3>
                </div>
            </body>
            </html>
        ";

                // HTML content for owner email
                string ownerEmailContent = @"
            <html>
                <head>
                    <style>
                        /* CSS styles for email */
                        body {
                            font-family: Arial, sans-serif;
                            background-color: #f0f0f0;
                            padding: 20px;
                        }
                        .order-summary {
                            background-color: #ffffff;
                            padding: 15px;
                            border-radius: 5px;
                            box-shadow: 0 0 10px rgba(0,0,0,0.1);
                            margin-bottom: 20px;
                            border: 2px solid #ff6600; /* Add border */
                            overflow: auto; /* Clear floats */
                        }
                        .order-summary h2 {
                            color: #ff6600;
                        }
                        .order-summary p {
                            color: #333333;
                            margin: 5px 0; /* Add spacing between paragraphs */
                            clear: both; /* Ensure each paragraph starts below any floated elements */
                        }
                        .order-summary img {
                            float: left; /* Float image to the left */
                            margin-right: 20px; /* Space between image and text */
                            margin-bottom: 10px; /* Space between image and surrounding content */
                            max-width: 100px; /* Adjust as needed */
                            height: auto; /* Maintain aspect ratio */
                        }
                        .order-summary p strong {
                            display: inline-block;
                            width: 180px; /* Adjust width for label alignment */
                        }
                        .order-details {
                            display: inline-block;
                            vertical-align: top;
                        }
                    </style>
                </head>
                <body>
                    <div class='order-summary'>
                        <img src='https://reactdotnetimages.blob.core.windows.net/reactdotnet/cake-logo.jpg' alt='Order Image'>
                        <h2>New Order Received</h2>
                        <p><strong>Order Id:</strong> <span class='order-details'>" + result.OrderHeader.Id + @"</span></p>
                        <p><strong>Customer Name:</strong> <span class='order-details'>" + result.OrderHeader.Name + @"</span></p>
                        <p><strong>Customer Contact Number:</strong> <span class='order-details'>" + result.OrderHeader.PhoneNumber + @"</span></p>
                        <p><strong>Customer Address:</strong> <span class='order-details'>" + result.OrderHeader.StreetAddress + @"</span></p>
                        <p><strong>Customer Postal Code:</strong> <span class='order-details'>" + result.OrderHeader.PostalCode + @"</span></p>
                        <p><strong>Order Date:</strong> <span class='order-details'>" + result.OrderHeader.OrderDate + @"</span></p>
                        <h3><strong>Bill Amount:</strong> Rs. <span class='order-details'>" + result.OrderHeader.OrderTotal + @"</span></h3>
                    </div>
                </body>
            </html>
        ";

                // Send emails with HTML content
                await _emailSender.SendEmailAsync(orderDTO.OrderHeader.Email, "Cake'O Clock Order Confirmation", customerEmailContent);
                await _emailSender.SendEmailAsync(ownerEmail, "New Order Cake'O Clock", ownerEmailContent);

                return Ok(result);
            }

            return BadRequest(new ErrorModelDTO()
            {
                ErrorMessage = "Failed to create the order"
            });
        }


        [HttpPost]
        [ActionName("paymentsuccessful")]
        public async Task<IActionResult> PaymentSuccessful([FromBody] OrderHeaderDTO orderHeaderDTO)
        {
            var service = new SessionService();
            var sessionDetails = service.Get(orderHeaderDTO.SessionId);
            


			if (sessionDetails.PaymentStatus == "paid")
            {
                var result = await _orderRepository.MarkPaymentSuccessful(orderHeaderDTO.Id, sessionDetails.PaymentIntentId);

                if (result != null)
                {
                    await _emailSender.SendEmailAsync(orderHeaderDTO.Email, "Cake'O Clock Order Confirmation",
                        "New Order has been created: " + result.Id);

					

					return Ok(result);
                }

                return BadRequest(new ErrorModelDTO()
                {
                    ErrorMessage = "Can not mark payment as successful"
                });
            }

            return BadRequest();
        }


        [HttpGet("{userEmail}")]
        public async Task<IActionResult> GetOrdersByEmail(string userEmail)
        {
            var orders = await _orderRepository.GetAllLoadedByEmail(userEmail);
            return Ok(orders);
        }


    }
}




//     [HttpPost]
//     [ActionName("Create")]
//     public async Task<IActionResult> Create([FromBody] OrderDTO orderDTO)
//     {
//         // Assuming that the orderDTO is already validated and contains necessary information
//         orderDTO.OrderHeader.OrderDate = DateTime.Now;

//         var result = await _orderRepository.Create(orderDTO);
//var ownerEmail = "cakeoclockbakery123@gmail.com";

//if (result != null)
//         {
//             await _emailSender.SendEmailAsync(orderDTO.OrderHeader.Email, "Cake'O Clock Order Confirmation",
//                 "New Order has been created by you! Your Order Id is : " + result.OrderHeader.Id); // + 

//	await _emailSender.SendEmailAsync(ownerEmail, "New Order Cake'O Clock",
//			"New Order has been created! Under the order Id :" + result.OrderHeader.Id);

//	return Ok(result);
//         }

//         return BadRequest(new ErrorModelDTO()
//         {
//             ErrorMessage = "Failed to create the order"
//         });
//     }


//[HttpPost]
//[ActionName("Create")]
//public async Task<IActionResult> Create([FromBody] OrderDTO orderDTO)
//{
//    if (orderDTO == null || orderDTO.OrderHeader == null)
//    {
//        return BadRequest(new ErrorModelDTO()
//        {
//            ErrorMessage = "OrderDTO or OrderHeader is null",
//            StatusCode = StatusCodes.Status400BadRequest
//        });
//    }

//    // Assuming that the orderDTO is already validated and contains necessary information
//    if (orderDTO.OrderHeader != null)
//    {
//        orderDTO.OrderHeader.OrderDate = DateTime.Now;
//    }

//    var result = await _orderRepository.Create(orderDTO);
//    var ownerEmail = "cakeoclockbakery123@gmail.com";

//    if (result != null && result.OrderHeader != null) // Ensure result and result.OrderHeader are not null
//    {
//        await _emailSender.SendEmailAsync(orderDTO.OrderHeader.Email, "Cake'O Clock Order Confirmation",
//            "<h2>New Order has been created by you! </h2>" +
//            "<p>Your Order Id: " + result.OrderHeader.Id + "</p>" +
//            "<p>Customer Name: " + result.OrderHeader.Name + "</p>" +
//            "<p>Customer Contact Number: " + result.OrderHeader.PhoneNumber + "</p>" +
//            "<p>Customer Address: " + result.OrderHeader.StreetAddress + "</p>" +
//            "<p>Customer Postal Code: " + result.OrderHeader.PostalCode + "</p>" +
//            "<p>Order Date: " + result.OrderHeader.OrderDate + "</p>" +
//            "<h3>Bill Amount: Rs." + result.OrderHeader.OrderTotal + "</h3>"

//            );

//        await _emailSender.SendEmailAsync(ownerEmail, "New Order Cake'O Clock",
//            "<h2>New Order has been created! </h2>" +
//            "<p>Under the Order Id: " + result.OrderHeader.Id + "</p>" +
//            "<p>Customer Name: " + result.OrderHeader.Name + "</p>" +
//            "<p>Customer Contact Number: " + result.OrderHeader.PhoneNumber + "</p>" +
//            "<p>Customer Address: " + result.OrderHeader.StreetAddress + "</p>" +
//            "<p>Customer Postal Code: " + result.OrderHeader.PostalCode + "</p>" +
//            "<p>Order Date: " + result.OrderHeader.OrderDate + "</p>" +
//            "<h3>Bill Amount: Rs." + result.OrderHeader.OrderTotal + "</h3>"
//            );


//        return Ok(result);
//    }

//    return BadRequest(new ErrorModelDTO()
//    {
//        ErrorMessage = "Failed to create the order"
//    });
//}