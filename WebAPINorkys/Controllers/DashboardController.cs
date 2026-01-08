using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NorkysAPI.Interfaces;

namespace WebAPINorkys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardDAO _dashboardDAO;

        public DashboardController(IDashboardDAO dashboardDAO)
        {
            _dashboardDAO = dashboardDAO;
        }
        //Endpoint 1 – Total vs Día(barras)
        [HttpGet("ventas-por-dia")]
        public async Task<IActionResult> GetVentasPorDia([FromQuery] int dias = 7)
        {
            var data = await _dashboardDAO.ObtenerVentasPorDiaAsync(dias);
            return Ok(data);
        }
        //Endpoint 2 – Métodos de pago (pie)
        [HttpGet("metodos-pago")]
        public async Task<IActionResult> GetMetodosPago()
        {
            var data = await _dashboardDAO.ObtenerMetodosPagoAsync();
            return Ok(data);
        }
        // Endpoint 3️ – Top productos vendidos por cantidad (últimos N días)
        [HttpGet("top-productos-vendidos")]
        public async Task<IActionResult> GetTopProductosVendidos(
            [FromQuery] int dias = 30,
            [FromQuery] int top = 5
        )
        {
            var data = await _dashboardDAO.ObtenerTopProductosVendidosAsync(dias, top);
            return Ok(data);
        }

        //Endpoint 4 – Top productos por ganancia (últimos N días)
        [HttpGet("top-productos-ganancia")]
        public async Task<IActionResult> GetTopProductosGanancia(
        [FromQuery] int dias = 30,
        [FromQuery] int top = 5)
        {
            var data = await _dashboardDAO.ObtenerTopProductosGananciaAsync(dias, top);
            return Ok(data);
        }
        //Endpoint 5 – Top productos por ganancia por fecha
        [HttpGet("top-productos-ganancia-fecha")]
        public async Task<IActionResult> GetTopProductosGananciaPorFecha([FromQuery] DateTime fecha)
        {
            var data = await _dashboardDAO.ObtenerTopProductosGananciaPorFechaAsync(fecha);
            return Ok(data);
        }
        //Endpoint 6 – Fechas disponibles (select HTML)
        [HttpGet("fechas")]
        public async Task<IActionResult> GetFechas([FromQuery] int top = 15)
        {
            var data = await _dashboardDAO.ObtenerFechasDisponiblesAsync(top);
            return Ok(data);
        }
        //Endpoint 7 - KPIs
        // KPI Dashboard
        [HttpGet("kpis")]
        public async Task<IActionResult> GetKpis([FromQuery] int dias = 7)
        {
            var data = await _dashboardDAO.ObtenerKpisAsync(dias);
            return Ok(data);
        }

    }
}
