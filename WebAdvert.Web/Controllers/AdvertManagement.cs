using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdvertApi.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models;
using WebAdvert.Web.ServiceClients;
using WebAdvert.Web.Services;

namespace WebAdvert.Web.Controllers
{
    public class AdvertManagement : Controller
    {
        private readonly IFileUploader _fileUploader;
        private readonly IAdvertApiClient _advertApiClient;
        private readonly IMapper _mapper;

        public AdvertManagement(IFileUploader fileUploader, IAdvertApiClient advertApiClient, IMapper mapper)
        {
            _fileUploader = fileUploader;
            _advertApiClient = advertApiClient;
            _mapper = mapper;
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
                // You must make a call to Advert API, create the advertisement in the database and return the ID
                var createAdvertModel = _mapper.Map<CreateAdvertModel>(model);
                var apiCallResponse = await _advertApiClient.Create(createAdvertModel);

                //Assumes that the call to Api is successful for sample
                var id = apiCallResponse.Id;

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

                        //If upload to S3 is successful then call the advert api again
                        //with the file path in S3 and the status as active
                        var confirmModel = new ConfirmAdvertRequest
                        {
                            Id = id,
                            FilePath = filePath,
                            Status = AdvertStatus.Active
                        };

                        var canConfirm = await _advertApiClient.Confirm(confirmModel);

                        if (!canConfirm)
                        {
                            throw new Exception($"Cannot confirm advert of id = {id}");
                        }

                        //Call Advert Api and confirm the advertisement
                        return RedirectToAction("Index", controllerName: "Home");
                    }
                    catch (Exception e)
                    {
                        //If upload to S3 is successful then call the adevert api again
                        var pendingModel = new ConfirmAdvertRequest
                        {
                            Id = id,
                            Status = AdvertStatus.Pending
                        };

                       await _advertApiClient.Confirm(pendingModel);
                        Console.WriteLine(e);
                    }
                }
            }

            return View(model);
        }
    }
}
