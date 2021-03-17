namespace ClassMonitor3.Util
{
    public class ComboItem
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public override string ToString() {
            return Value;
        }
        public ComboItem(string value,string text) {
            Value = value;
            Text = text;
        }
    }
}
