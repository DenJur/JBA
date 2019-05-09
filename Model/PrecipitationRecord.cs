using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JBA.Model
{
    class PrecipitationRecord
    {
        [Key] public int Id { get; set; }
        public int Xref { get; set; }
        public int Yref { get; set; }
        public DateTime Date { get; set; }
        public int Value { get; set; }

    }
}