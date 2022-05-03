using Microsoft.IdentityModel.Tokens;
using Sabio.Data;
using Sabio.Data.Providers;
using Sabio.Models;
using Sabio.Models.Domain;
using Sabio.Models.Requests.Messages;
using Sabio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabio.Services.Messages
{
    public class MessageService : IMessageService
    {
        IDataProvider _data = null;
        IUserMapper _userMapper = null;
        public MessageService(IDataProvider data, IUserMapper mapper)
        {
            _data = data;
            _userMapper = mapper;
        }

        public Message Get(int id)
        {

            string procName = "[dbo].[Messages_Select_ById_V2]";
            Message message = null;

            _data.ExecuteCmd(procName, delegate (SqlParameterCollection paramCollection)
            {

                paramCollection.AddWithValue("@Id", id);

            }, delegate (IDataReader reader, short set)
            {
                int index = 0;
                message = MappingMessage(reader, ref index);
            }
            );

            return message;
        }

        public Paged<Message> GetAll(int pageIndex, int pageSize)
        {
            Paged<Message> pagedList = null;
            List<Message> list = null;
            string procName = "[dbo].[Messages_SelectAll_V2]";
            int totalCount = 0;

            _data.ExecuteCmd(
               procName,
                (param) =>

                {
                    param.AddWithValue("@pageIndex", pageIndex);
                    param.AddWithValue("@pageSize", pageSize);
                },
                (reader, recordSetIndex) =>
                {
                    int index = 0;
                    Message message = new Message();
                    message = MappingMessage(reader, ref index);
                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(index++);
                    }

                    if (list == null)
                    {
                        list = new List<Message>();
                    }

                    list.Add(message);
                }
                );
            if (list != null)
            {
                pagedList = new Paged<Message>(list, pageIndex, pageSize, totalCount);
            }
            return pagedList;
        }

        public Paged<Message> GetBySender(int senderId, int pageIndex, int pageSize)  
        {
            Paged<Message> pagedList = null;
            List<Message> list = null;
            string procName = "[dbo].[Messages_Select_ByCreatedBy_V2]";
            int totalCount = 0;

            _data.ExecuteCmd(
               procName,
                (param) =>

                {
                    param.AddWithValue("@SenderId", senderId);
                    param.AddWithValue("@pageIndex", pageIndex);
                    param.AddWithValue("@pageSize", pageSize);
                },
                (reader, recordSetIndex) =>
                {
                    int index = 0;
                    Message message = new Message();
                    message = MappingMessage(reader, ref index);
                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(index++);
                    }

                    if (list == null)
                    {
                        list = new List<Message>();
                    }

                    list.Add(message);
                }
                );
            if (list != null)
            {
                pagedList = new Paged<Message>(list, pageIndex, pageSize, totalCount);
            }  
            return pagedList;
        }    

        public Paged<Message> GetByRecipient(int recipientId, int pageIndex, int pageSize)
        {
            Paged<Message> pagedList = null;
            List<Message> list = null;
            string procName = "[dbo].[Messages_Select_ByRecipientId_V2]";
            int totalCount = 0;

            _data.ExecuteCmd(
               procName,
                (param) =>

                {
                    param.AddWithValue("@RecipientId", recipientId);
                    param.AddWithValue("@pageIndex", pageIndex);
                    param.AddWithValue("@pageSize", pageSize);
                },
                (reader, recordSetIndex) =>
                {
                    int index = 0;
                    Message message = new Message();
                    message = MappingMessage(reader, ref index);
                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(index++);
                    }

                    if (list == null)
                    {
                        list = new List<Message>();
                    }

                    list.Add(message);
                }
                );
            if (list != null)
            {
                pagedList = new Paged<Message>(list, pageIndex, pageSize, totalCount);
            }
            return pagedList;
        }

        public void Delete(int id)
        {
            string procName = "[dbo].[Messages_Delete_ById]";

            _data.ExecuteNonQuery(procName, delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", id);
            });
        }

        public Message Create(MessageAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[Messages_Insert]";
            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonParams(model, col);

                SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;

                col.Add(idOut);

            }, returnParameters: delegate (SqlParameterCollection returnCollection)
            {
                object oId = returnCollection["@Id"].Value;

                int.TryParse(oId.ToString(), out id);      

            });

            Message message = Get(id);

            return message;    
        }

        public void Update(MessageUpdateRequest model)                
        {
            string procName = "[dbo].[Messages_Update]";
            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonParams(model, col);
                col.AddWithValue("@Id", model.Id); 

            },
            returnParameters: null);
        }

        private static void AddCommonParams(MessageAddRequest model, SqlParameterCollection col)
        {

            col.AddWithValue("@Message", model.Message);
            col.AddWithValue("@Subject", string.IsNullOrEmpty(model.Subject) ? (object)DBNull.Value : model.Subject);
            col.AddWithValue("@RecipientId", model.RecipientId);
            col.AddWithValue("@SenderId", model.SenderId);
            col.AddWithValue("@DateSent", model.DateSent);
            col.AddWithValue("@DateRead", model.DateRead == null ? (object)DBNull.Value : model.DateRead);

        }

        private Message MappingMessage(IDataReader reader, ref int index)
        {
            Message message = new Message();
            message.Id = reader.GetSafeInt32(index++);
            message.MessageContent = reader.GetSafeString(index++);  
            message.Subject = reader.GetSafeString(index++);
            message.Recipient = _userMapper.Map(reader, ref index);
            message.Sender = _userMapper.Map(reader, ref index);
            message.DateSent = reader.GetSafeDateTimeNullable(index++);
            message.DateRead = reader.GetSafeDateTimeNullable(index++);
            message.DateCreated = reader.GetSafeDateTime(index++);
            message.DateModified = reader.GetSafeDateTime(index++);

            return message;
        }
    }
}
