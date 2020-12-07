using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FileUploadExample.Data;
using FileUploadExample.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;

namespace FileUploadExample.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductDbContext _context;
        private readonly IConfiguration _config;

        public ProductsController(ProductDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Product.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(8388608)] // 8MB
        public async Task<IActionResult> Create( Product product)
        {
            if (ModelState.IsValid)
            {
                // TODO: Validate Product Photo
                IFormFile photo = product.ProductPhoto;

                if(photo.Length > 0)
                {
                    // add error message
                    // return view
                }

                string extension = Path.GetExtension(photo.FileName).ToLower();
                string[] permittedExtensions = { ".png", ".gif", ".jpg" };
                if (!permittedExtensions.Contains(extension))
                {
                    // add erro0r message
                    // return view
                }


                // Generate unqiue file name
                // Save photo to storage
                string acc = _config.GetSection("StorageAccountName").Value;
                string key = _config.GetSection("StorageAccountKey").Value;

                // TODO: Use real connection string for dev that way we can swap out for production
                BlobServiceClient blobService = new BlobServiceClient("UseDevelopmentStorage=true");

                // Create container to hold BLOBs
                // TODO: Handle exception if contianer already exists
                BlobContainerClient containerClient =
                    await blobService.CreateBlobContainerAsync("photos");

                // Add BLOV to container
                string newFileName = Guid.NewGuid().ToString() + extension;
                BlobClient blobClient = containerClient.GetBlobClient(newFileName);

                await blobClient.UploadAsync(product.ProductPhoto.OpenReadStream());

                product.PhotoUrl = newFileName;
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Price,PhotoUrl")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            _context.Product.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
    }
}
