// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;

// namespace backend.Controllers.Admin
// {
//     public class BookingsController : Controller
//     {
//         [Route("Admin/[controller]")]
//         // GET: BookingsController
//         public ActionResult Index()
//         {
//             return View();
//         }

//         // GET: BookingsController/Details/5
//         public ActionResult Details(int id)
//         {
//             return View();
//         }

//         // GET: BookingsController/Create
//         public ActionResult Create()
//         {
//             return View();
//         }

//         // POST: BookingsController/Create
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public ActionResult Create(IFormCollection collection)
//         {
//             try
//             {
//                 return RedirectToAction(nameof(Index));
//             }
//             catch
//             {
//                 return View();
//             }
//         }

//         // GET: BookingsController/Edit/5
//         public ActionResult Edit(int id)
//         {
//             return View();
//         }

//         // POST: BookingsController/Edit/5
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public ActionResult Edit(int id, IFormCollection collection)
//         {
//             try
//             {
//                 return RedirectToAction(nameof(Index));
//             }
//             catch
//             {
//                 return View();
//             }
//         }

//         // GET: BookingsController/Delete/5
//         public ActionResult Delete(int id)
//         {
//             return View();
//         }

//         // POST: BookingsController/Delete/5
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public ActionResult Delete(int id, IFormCollection collection)
//         {
//             try
//             {
//                 return RedirectToAction(nameof(Index));
//             }
//             catch
//             {
//                 return View();
//             }
//         }
//     }
// }
