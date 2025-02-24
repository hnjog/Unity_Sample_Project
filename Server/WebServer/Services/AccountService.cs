using GameDB;

namespace WebServer.Services
{
    public class AccountService
    {
        GameDbContext _dbContext;

        public AccountService(GameDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        int _idGenerator = 1;

        public int GenerateAccountId()
        {
            _idGenerator++;


            // 변화점 저장
            _dbContext.SaveChanges();

            return _idGenerator;
        }
    }
}
