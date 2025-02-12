using Microsoft.AspNetCore.Mvc;
using WebServer.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebServer.Controllers
{
    [ApiController] // route 관련 클래스로 인지하기에 frameWork에서 객체 생성을 해준다
    [Route("test")]
    public class TestController : ControllerBase
    {
        AccountService _service;

        public TestController(AccountService service)
        {
            _service = service;
        }

        // ASP.NET CORE < WEB
        // ENTITY FRAMEWORK CORE < DB(ORM)

        // input(server에서 받음)
        [HttpPost]
        [Route("testPost")]
        public TestPacketRes TestPost([FromBody] TestPacketReq value)
        {
            // 가장 광범위하게 사용하며, 게임 서버와 비슷
            TestPacketRes result = new TestPacketRes();
            result.success = true;

            int id = _service.GenerateAccountId();
            // json 변화 작업은 내부 framework에서 변환
            return result;
        }

    }
}
