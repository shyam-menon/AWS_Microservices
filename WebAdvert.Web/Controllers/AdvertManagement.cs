using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models;
using WebAdvert.Web.Services;

namespace WebAdvert.Web.Controllers
{
    public class AdvertManagement : Controller
    {
        private readonly IFileUploader _fileUploader;

        public AdvertManagement(IFileUploader fileUploader)
        {
            _fileUploader = fileUploader;
        }
        
        public IActionResult Create(CreateAdvertViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAdvertViewModel model, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                var id = "11111";
                // You must make a call to Advert API, create the advertisement in the database and return the ID

                var fileName = "";
                if (imageFile != null)
                {
                    fileName = !string.IsNullOrEmpty(imageFile.FileName) ?
                        Path.GetFileName(imageFile.FileName) : id;
                    var filePath = $"{id}/{fileName}";

                    try
                    {
                        using (var readStream =imageFile.OpenReadStream())
                        {
                            //Upload to S3
                            var result = await _fileUploader.UploadFileAsync(filePath, readStream)
                                .ConfigureAwait(continueOnCapturedContext: false);
                            if (!result)
                                throw new Exception
                                (message: "Could not upload the image to file repository. See logs for details");
                        }

                        //Call Advert Api and confirm the advertisement
                        return RedirectToAction("Index", controllerName: "Home");
                    }
                    catch (Exception e)
                    {
                        //Call Advert Api and cancel the advertisement
                        Console.WriteLine(e);
                    }
                }
            }

            return View(model);
        }
    }
}
