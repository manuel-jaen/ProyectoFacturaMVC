
namespace FacturaProyectMVC.Models
{
    using Microsoft.Ajax.Utilities;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Pago
    {

    public int Id { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public System.DateTime Fecha { get; set; }
        public int Factura { get; set; }
        
        public decimal Valor { get; set; }
        public virtual Factura Factura1 { get; set; }

    }
}
