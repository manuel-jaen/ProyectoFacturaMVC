using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml.Schema;
using FacturaProyectMVC.Models;
using Microsoft.Ajax.Utilities;

namespace FacturaProyectMVC.Controllers
{
    public class FacturasController : Controller
    {
        private FacturacionEntities1 db = new FacturacionEntities1();

        // GET: Facturas
        public ActionResult Index(string BuscarNombre)
        {
            //tomar todas las facturas para hacer display de ellas
            var facturas = from seleccion in db.Facturas select seleccion;

            /*logica de barra de busqueda
            Verificamos si se busca algo*/
            if (!String.IsNullOrEmpty(BuscarNombre))
            {
                facturas = facturas.Where(c => c.Nombre.Contains(BuscarNombre));
            }

            //para todas las facturas
            foreach(var factura in facturas)
            {
                //conseguimos la lista de articulos
                var articulo = db.Articulos.Where(b => b.Factura==factura.Id).ToList();
                decimal tempsum = 0;
                //tenemos nuestra sumatoria total
                foreach (var item in articulo)
                {
                    tempsum = (decimal)(tempsum + item.Total);
                }
                //al terminar de sumar todos los totales de los articulos, le asignamos el valor total a la factura
                var tempObj = db.Facturas.First(x => x.Id == factura.Id);
                tempObj.Total = tempsum;

                //hacemos el remplaze del total en la DB
                db.Facturas.Attach(tempObj);
                var entry = db.Entry(tempObj);
                entry.Property(e => e.Total).IsModified = true;
            }

            //tambien necesitamos sacar el saldo pago
            //para todas las facturas
            foreach (var factura in facturas)
            {
                //conseguimos la lista de articulos
                var pago = db.Pagos.Where(b => b.Factura == factura.Id).ToList();
                decimal tempsum = 0;
                //tenemos nuestra sumatoria total
                foreach (var item in pago)
                {
                    tempsum = (decimal)(tempsum + item.Valor);
                }
                //al terminar de sumar todos los totales de los articulos, le asignamos el valor total a la factura
                var tempObj = db.Facturas.First(x => x.Id == factura.Id);
                tempObj.SaldoPago =tempsum;

                //hacemos el remplaze del total en la DB
                db.Facturas.Attach(tempObj);
                var entry = db.Entry(tempObj);
                entry.Property(e => e.SaldoPago).IsModified = true;
            }
            //salvamos los cambios en la base de datos
            db.SaveChanges();

            return View(facturas);
        }

        // GET: Facturas/RealizarPago
        public ActionResult RealizarPago(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factura factura = db.Facturas.Find(id);
            if (factura == null)
            {
                return HttpNotFound();
            }

            //metemos el id de la factora en una lista de seleccion usando viewbag
            ViewBag.Factura = new SelectList(db.Facturas.Where(g=>g.Id==factura.Id), "Id", "Nombre");
            //pasamos el saldopendiente para usarlo en la validacion tambien
            ViewBag.Pagomaximo = factura.SaldoPendiente;
            return View();
            
        }

        // POST: Pagos/RealizarPago
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RealizarPago([Bind(Include = "Id,Fecha,Valor,Factura")] Pago pago)
        {
            if (ModelState.IsValid)
            {
                //consigamos el id de la factura que se supone que estaba pagando
                Factura factura = db.Facturas.Find(pago.Factura);
                ViewBag.Pagomaximo = factura.SaldoPendiente;
                ViewBag.PagoRecibido = pago.Valor;
                //si no se paga la cantidad adecuada
                if (pago.Valor > ViewBag.Pagomaximo || pago.Valor <= 1)
                {
                    //no guardamos el pago y le enviamos a una vista que le informe que la cantidad no es valida
                    return RedirectToAction("PagoFallido", pago);
                }
                //guardamos el pago
                db.Pagos.Add(pago);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            //si el modelo no es valido
            ViewBag.Factura = new SelectList(db.Facturas, "Id", "Nombre", pago.Factura);
            return View(pago);
        }

        public ActionResult PagoFallido(Pago pago) 
        {
            //agarramos el valor de el saldopendiente y la cantidad paga
            Factura factura = db.Facturas.Find(pago.Factura);
            ViewBag.Pagomaximo = factura.SaldoPendiente;
            ViewBag.PagoRecibido = pago.Valor;
            return View();
        }

        // GET: Facturas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Factura factura = db.Facturas.Find(id);
            if (factura == null)
            {
                return HttpNotFound();
            }
            return View(factura);
        }

        // GET: Facturas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pago pago = db.Pagos.Find(id);
            if (pago == null)
            {
                return HttpNotFound();
            }
            return View(pago);
        }

        // POST: Facturas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Pago pago = db.Pagos.Find(id);
            db.Pagos.Remove(pago);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
