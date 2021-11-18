namespace Andy.X.Connect.Model.Postgres
{
    public class Payload
    {
        public string Table { get; set; }
        public string Action { get; set; }
        public object Data { get; set; }
    }
}
