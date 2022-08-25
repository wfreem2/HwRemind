namespace HwRemind.Configs
{
    public class JWTConfig
    {
        public string secret { get; set; }
        public string iss { get; set; }
        public string aud { get; set; }
        public string alg { get; set; }
        public int exp { get; set; }
        public int refreshExp { get; set; }
    }
}
