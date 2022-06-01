using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Helper
{
    public class CustomException: Exception
    {
        public CustomException(string account):base() { Data.Add("account", account); }
        public override string Message => string.Format("{0}", Data["account"]);
    }

    public class FatalException : CustomException
    {
        public FatalException(string account) : base(account) { }
    }

    public class PasswordException: FatalException {
        public PasswordException(string account) : base(account) { }

        public override string Message => string.Format("账号{0}用户名或密码错误", Data["account"]);
    }

    public class AlreadyLoginException: FatalException //CustomException
    {
        public AlreadyLoginException(string account) : base(account) { }

        public override string Message => string.Format(@"该账号{0}已经登录请用其它账号采集", Data["account"]);
    }

    public class AccountNotFoundException : FatalException
    {
        public AccountNotFoundException(string account) : base(account) { }

        public override string Message => string.Format("该账号{0}不存在请查实后修改", Data["account"]);
    }

    public class AccountORPasswordErrorException : FatalException
    {
        public AccountORPasswordErrorException(string account) : base(account) { }

        public override string Message => string.Format("该账号{0}不存在或密码错误", Data["account"]);
    }

    public class AccountLoginTooMuchException : FatalException
    {
        public AccountLoginTooMuchException(string account) : base(account) { }

        public override string Message => string.Format("该账号{0}使用验证码登录太频繁", Data["account"]);
    }

    public class LoginTimeoutException : FatalException
    {
        public LoginTimeoutException(string account) : base(account) { }

        public override string Message => string.Format("该账号{0}登录超时！请重新手动登录。", Data["account"]);
    }

    public class ParseCaptchaException : CustomException
    {
        public ParseCaptchaException(string account) : base(account) { }

        public override string Message => string.Format("该账号{0}识别验证码失败！", Data["account"]);
    }

    public class ParseException: CustomException
    {
        public ParseException(string account) : base(account) { }

        public override string Message => string.Format("账号{0}所在网站解析出错", Data["account"]);
    }
    public class FatalParseException : FatalException
    {
        public FatalParseException(string account) : base(account) { }

        public override string Message => string.Format("账号{0}所在网站解析出错", Data["account"]);
    }

    public class CaptchaNeedMoneyException : FatalException //CustomException
    {
        public CaptchaNeedMoneyException(string account) : base(account) { }

        public override string Message => string.Format("验证码系统账号{0}需要充值", Data["account"]);
    }
    public class CaptchaNoAccountException : FatalException //CustomException
    {
        public CaptchaNoAccountException(string account) : base(account) { }

        public override string Message => string.Format("无验证码系统账号");
    }

    public class ConnectionException : CustomException//FatalException
    {
        public ConnectionException(string url) : base(url) { }

        public override string Message => string.Format("远程服务器({0})连接异常！", Data["account"]);
    }
}
