namespace CAPSI.Sante.API.ModelsReponse
{
    public class BooleanResponse
    {
        public bool Success { get; set; }
        public bool Data { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string RequestId { get; set; }
    }
}
