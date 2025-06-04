using FhEnums;
using FrontendHelper.Controllers.AuthorizationAttributes;
using FrontendHelper.Models;
using Microsoft.AspNetCore.Mvc;

namespace FrontendHelper.Controllers
{
    public class QrCodeController : Controller
    {
        private readonly QrCodeService _qrCodeService;
        public QrCodeController(QrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
        }


        [HasPermission(Permission.CanGenerateQrCode)]
        public IActionResult GenerateQrCode(QrCodeViewModel viewModel)
        {
            if (!string.IsNullOrWhiteSpace(viewModel.Resource))
            {
                var pngBytes = _qrCodeService.Generate(viewModel.Resource);
                var base64 = Convert.ToBase64String(pngBytes);
                viewModel.QrCodeImage = $"data:image/png;base64,{base64}";
            }
            return View(viewModel);
        }

    }
}
