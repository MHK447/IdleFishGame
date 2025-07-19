// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("ZWWLDNwmUm7KITcPZiArve3wgwV1B4tBIcjDHlw1nY5dNh4Y+0AtLtadl5I5ugMk20e+qffO7UhzBijhb5LMXHbSnWMOyM3G+bRxgqUSqVdZB6hT51gApzUAYxnv2yGc8RB42P8tuk4Qj2GBybXOdHq3CGfiUXQPJRvx0kaZYD5dezDhZ/76VmlthLrxm97jyr7v6r2ctief5nLFekgGWAmdGp4og+ynBZdT39/kSEcFWcNL7tIAKDfKVZozR7az9x3xq2QzpA4VlpiXpxWWnZUVlpaXR9JoBytqIgyl2/9AhADIZMAnu/YAQiywVlX7jOXC85DLSa39k6DW9s55wsebUDCnFZa1p5qRnr0R3xFgmpaWlpKXlKOdIp71IbcbNJWUlpeW");
        private static int[] order = new int[] { 6,3,13,4,10,12,13,7,12,11,10,11,12,13,14 };
        private static int key = 151;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
