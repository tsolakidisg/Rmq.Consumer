using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Rmq.Consumer
{
    class FullModel
    {
        [Required]
        public int OrderId { get; set; }
        public int Fees { get; set; }
        public string CustomerName { get; set; }
        public string OrderStatus { get; set; }
    }
}
