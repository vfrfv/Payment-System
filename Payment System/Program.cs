using System;
using System.Security.Cryptography;
using System.Text;

namespace Payment_System
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Order order = new Order(111, 12);

            Md5HashService md5 = new Md5HashService();
            Sha1HashService sha1 = new Sha1HashService();

            var paymentSystems = new IPaymentSystem[]
            {
                new Mastercard(md5),
                new Qiwi(md5),
                new Mir(sha1, "sss")
            };

            foreach (var paymentSystem in paymentSystems)
            {
                string payingLink = paymentSystem.GetPayingLink(order);

                Console.WriteLine(payingLink);
            }
        }
    }

    public class Order
    {
        public Order(int id, int amount)
        {
            Id = id;
            Amount = amount;
        }

        public int Id { get; private set; }
        public int Amount { get; private set; }
    }

    public interface IPaymentSystem
    {
        string GetPayingLink(Order order);
    }

    public interface IHashService
    {
        string ComputeHash(string data);
    }

    class Md5HashService : IHashService
    {
        public string ComputeHash(string data)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(data));

                return Convert.ToBase64String(hash);
            }
        }
    }

    class Sha1HashService : IHashService
    {
        public string ComputeHash(string data)
        {
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(data));

                return Convert.ToBase64String(hash);
            }
        }
    }

    class Mastercard : IPaymentSystem
    {
        private readonly IHashService _hashService;

        public Mastercard(IHashService hashService)
        {
            _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
        }

        public string GetPayingLink(Order order)
        {
            string hash = _hashService.ComputeHash(order.Id.ToString());

            return $"pay.system1.ru/order?amount={order.Amount}RUB&hash={hash}";
        }
    }

    class Qiwi : IPaymentSystem
    {
        private readonly IHashService _hashService;

        public Qiwi(IHashService hashService)
        {
            _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
        }

        public string GetPayingLink(Order order)
        {
            string hash = _hashService.ComputeHash($"{order.Id}{order.Amount}");

            return $"order.system2.ru/pay?hash={hash}";
        }
    }

    class Mir : IPaymentSystem
    {
        private readonly string _secretKey;
        private readonly IHashService _hashService;

        public Mir(IHashService hashService, string secretKey)
        {
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new ArgumentException($"\"{nameof(secretKey)}\" не может быть пустым или содержать только пробел.", nameof(secretKey));
            }

            _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
            _secretKey = secretKey;
        }

        public string GetPayingLink(Order order)
        {
            string hash = _hashService.ComputeHash(GetFinalData(order));

            return $"system3.com/pay?amount={order.Amount}&curency=RUB&hash={hash}";
        }

        private string GetFinalData(Order order)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(order.Amount);
            stringBuilder.Append(order.Id);
            stringBuilder.Append(_secretKey);

            return stringBuilder.ToString();
        }
    }
}
