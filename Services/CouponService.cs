using System.Collections.Generic;

namespace ECommerceWebApp.Services
{
    public class CouponService
    {
        private readonly Dictionary<string, decimal> _validCoupons = new()
        {
            { "SAVE10", 10 },
            { "OFF20", 20 },
            { "BIG30", 30 }
        };

        public bool IsValidCoupon(string code) => _validCoupons.ContainsKey(code.ToUpper());

        public decimal GetDiscountPercentage(string code)
        {
            return IsValidCoupon(code) ? _validCoupons[code.ToUpper()] : 0;
        }
    }
}
