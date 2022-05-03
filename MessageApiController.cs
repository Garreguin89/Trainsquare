using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Tasks;
using Microsoft.Extensions.Logging;
using Sabio.Services;
using Sabio.Services.Interfaces;
using Sabio.Web.Controllers;
using Sabio.Web.Models.Responses;
using System;
using Sabio.Models;
using Sabio.Models.Requests.Messages;
using Message = Sabio.Models.Domain.Message;
using Microsoft.AspNetCore.SignalR;
using Sabio.Web.Api.Hubs;
using Sabio.Web.Api.Hubs.Clients;

namespace Sabio.Web.Api.Controllers.Messages
{
    [Route("api/messages")]
    [ApiController]
    public class MessageApiController : BaseApiController  
    {
        private readonly IHubContext<ChatHub, IChatClient> _chatHub;
        private IMessageService _service = null;
        private IAuthenticationService<int> _authService = null;
        public MessageApiController(IMessageService service
            , IHubContext<ChatHub, IChatClient> chatHub
            , ILogger<MessageApiController> logger
            , IAuthenticationService<int> authService) : base(logger)
        {
            _chatHub = chatHub;
            _service = service;
            _authService = authService;
        }

        [HttpGet("{id:int}")]
        public ActionResult<ItemResponse<Message>> Get(int id)
        {
            int iCode = 200;
            BaseResponse response = null;
            try
            {
                Message message = _service.Get(id);
                if (message == null)
                {
                    iCode = 404;
                    response = new ErrorResponse("Application Resource not found");
                }
                else
                {
                    response = new ItemResponse<Message> { Item = message };
                }
            }
            catch (Exception ex)
            {
                iCode = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }

            return StatusCode(iCode, response);            
        }       

        [HttpGet("all")]
        public ActionResult<ItemResponse<Paged<Message>>> GetAll(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;
            try
            {
                Paged<Message> paged = _service.GetAll(pageIndex, pageSize);
                if (paged == null)
                {
                    code = 404;
                    response = new ErrorResponse("Records Not Found");
                }
                else
                {
                    response = new ItemResponse<Paged<Message>> { Item = paged };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }

        [HttpGet("sender/{senderId:int}")]
        public ActionResult<ItemResponse<Paged<Message>>> GetBySender(int senderId, int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;
            try
            {
                Paged<Message> paged = _service.GetBySender(senderId, pageIndex, pageSize);
                if (paged == null)
                {
                    code = 404;
                    response = new ErrorResponse("Records Not Found");
                }
                else
                {
                    response = new ItemResponse<Paged<Message>> { Item = paged };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }

        [HttpGet("recipient/{recipientId:int}")]
        public ActionResult<ItemResponse<Paged<Message>>> GetByRecipient(int recipientId, int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;
            try
            {
                Paged<Message> paged = _service.GetByRecipient(recipientId, pageIndex, pageSize);
                if (paged == null)
                {
                    code = 404;
                    response = new ErrorResponse("Records Not Found");
                }
                else
                {
                    response = new ItemResponse<Paged<Message>> { Item = paged };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }

        [HttpDelete("{id:int}")]
        public ActionResult<ItemsResponse<Message>> Delete(int id)
        {
            int icode = 200;
            BaseResponse response = null;
            try
            {
                _service.Delete(id);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                icode = 500;
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(icode, response);
        }

        [HttpPost]
        public ActionResult<ItemResponse<int>> Create(MessageAddRequest model)
        {
            ObjectResult result = null;
            try
            {
                Message message = _service.Create(model);
                ItemResponse<int> response = new ItemResponse<int> { Item = message.Id };
                _chatHub.Clients.All.ReceiveMessage(message);
          
                result = Created201(response);
            }
            catch (Exception ex)
            {
                base.Logger.LogError(ex.ToString());
                ErrorResponse response = new ErrorResponse(ex.Message);
                result = StatusCode(500, response);
            }
            
            return result;
        }

        [HttpPut("{id:int}")]
        public ActionResult<ItemResponse<int>> Update(MessageUpdateRequest model)
        {
            int code = 200;
            BaseResponse response = null;
            try
            {
                _service.Update(model);
                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(code, response);
        }


    }
}
