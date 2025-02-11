using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebServer.Controllers
{
    [ApiController]
    [Route("test")]
    public class TestController : ControllerBase
    {
        // input(server에서 받음)
        [HttpPost]
        [Route("testPost")]
        public TestPacketRes TestPost([FromBody] TestPacketReq value)
        {
            // 가장 광범위하게 사용하며, 게임 서버와 비슷
            TestPacketRes result = new TestPacketRes();
            result.success = true;

            // json 변화 작업은 내부 framework에서 변환
            return result;
        }

    }
}
