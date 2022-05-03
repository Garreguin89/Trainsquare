using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Sabio.Models.Requests.Messages
{
    public class MessageAddRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Message { get; set; }

        public string? Subject { get; set; }

        [Required]
        [Range(1, Int32.MaxValue)]
        public int RecipientId { get; set; }

        [Required]
        [Range(1, Int32.MaxValue)]
        public int SenderId { get; set; }

        public DateTime? DateSent { get; set; }

        public DateTime? DateRead { get; set; }
    }
}
