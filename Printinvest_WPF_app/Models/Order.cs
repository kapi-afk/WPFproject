using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Printinvest_WPF_app.Utilities;

namespace Printinvest_WPF_app.Models
{
    public class Order : INotifyPropertyChanged
    {
        private OrderStatus _status;
        private byte[] _problemPhoto;
        private bool _isDetailsExpanded;
        private decimal _estimatedPartsCost;
        private decimal _masterWorkCost;
        private decimal _estimatedRepairCost;
        private System.DateTime? _completedAt;
        private string _paymentMethod;
        private bool _isOnlinePaymentCompleted;
        private System.DateTime? _onlinePaymentPaidAt;

        public const string OnSitePaymentMethod = "Оплата на месте";
        public const string OnlinePaymentMethod = "Онлайн-оплата";

        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int? AssignedMasterId { get; set; }
        public User AssignedMaster { get; set; }
        public string DeviceType { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceModel { get; set; }
        public string ProblemDescription { get; set; }
        public string DeliveryMethod { get; set; }
        public string DeliveryAddress { get; set; }
        public string PublicNumber { get; set; }
        public byte[] ProblemPhoto
        {
            get => _problemPhoto;
            set
            {
                if (_problemPhoto != value)
                {
                    _problemPhoto = value;
                    OnPropertyChanged(nameof(ProblemPhoto));
                    OnPropertyChanged(nameof(HasProblemPhoto));
                }
            }
        }
        [NotMapped]
        public bool HasProblemPhoto => ProblemPhoto != null && ProblemPhoto.Length > 0;
        [NotMapped]
        public bool IsDetailsExpanded
        {
            get => _isDetailsExpanded;
            set
            {
                if (_isDetailsExpanded != value)
                {
                    _isDetailsExpanded = value;
                    OnPropertyChanged(nameof(IsDetailsExpanded));
                    OnPropertyChanged(nameof(DetailsButtonText));
                }
            }
        }
        [NotMapped]
        public string DetailsButtonText => IsDetailsExpanded
            ? App.GetString("HideButton", "Hide")
            : App.GetString("DetailsButton", "Details");
        [NotMapped]
        public string DisplayNumber => OrderPublicNumberService.GetOrCreate(this);
        [NotMapped]
        public bool IsOnlinePayment => string.Equals(PaymentMethod, OnlinePaymentMethod, StringComparison.Ordinal);
        [NotMapped]
        public bool CanShowOnlinePaymentButton => IsOnlinePayment && !IsOnlinePaymentCompleted;
        [NotMapped]
        public bool CanBeCancelledByClient =>
            !IsOnlinePaymentCompleted &&
            (Status == OrderStatus.Created ||
             Status == OrderStatus.Assigned ||
             Status == OrderStatus.Diagnosing);
        [NotMapped]
        public string PaymentMethodDisplay
        {
            get
            {
                var paymentMethod = string.IsNullOrWhiteSpace(PaymentMethod) ? OnSitePaymentMethod : PaymentMethod;
                if (string.Equals(paymentMethod, OnSitePaymentMethod, StringComparison.Ordinal))
                {
                    return App.GetString("PaymentMethodOnSite", "On-site payment");
                }

                if (string.Equals(paymentMethod, OnlinePaymentMethod, StringComparison.Ordinal))
                {
                    return App.GetString("PaymentMethodOnline", "Online payment");
                }

                return paymentMethod;
            }
        }
        [NotMapped]
        public string DeliveryMethodDisplay
        {
            get
            {
                if (string.Equals(DeliveryMethod, "Курьер", StringComparison.Ordinal))
                {
                    return App.GetString("DeliveryCourier", "Courier");
                }

                if (string.IsNullOrWhiteSpace(DeliveryMethod) || string.Equals(DeliveryMethod, "Самовывоз", StringComparison.Ordinal))
                {
                    return App.GetString("PickupText", "Pickup");
                }

                return DeliveryMethod;
            }
        }
        [NotMapped]
        public string PaymentStatusText => !IsOnlinePayment
            ? App.GetString("PaymentOnSiteStatus", "On-site payment")
            : (IsOnlinePaymentCompleted
                ? App.GetString("PaymentOnlineCompletedStatus", "Paid online")
                : App.GetString("PaymentOnlinePendingStatus", "Awaiting online payment"));
        [NotMapped]
        public string PaymentPaidAtText => OnlinePaymentPaidAt.HasValue
            ? OnlinePaymentPaidAt.Value.ToString("dd.MM.yyyy HH:mm")
            : App.GetString("PaymentNotPaid", "Not paid");
        public string ContactPhone { get; set; }
        public string ClientComment { get; set; }
        public string AdminComment { get; set; }
        public decimal EstimatedPartsCost
        {
            get => _estimatedPartsCost;
            set
            {
                if (_estimatedPartsCost != value)
                {
                    _estimatedPartsCost = value;
                    OnPropertyChanged(nameof(EstimatedPartsCost));
                }
            }
        }

        public decimal MasterWorkCost
        {
            get => _masterWorkCost;
            set
            {
                if (_masterWorkCost != value)
                {
                    _masterWorkCost = value;
                    OnPropertyChanged(nameof(MasterWorkCost));
                }
            }
        }

        public decimal EstimatedRepairCost
        {
            get => _estimatedRepairCost;
            set
            {
                if (_estimatedRepairCost != value)
                {
                    _estimatedRepairCost = value;
                    OnPropertyChanged(nameof(EstimatedRepairCost));
                }
            }
        }

        public System.DateTime? CompletedAt
        {
            get => _completedAt;
            set
            {
                if (_completedAt != value)
                {
                    _completedAt = value;
                    OnPropertyChanged(nameof(CompletedAt));
                }
            }
        }

        public string PaymentMethod
        {
            get => _paymentMethod;
            set
            {
                if (_paymentMethod != value)
                {
                    _paymentMethod = value;
                    OnPropertyChanged(nameof(PaymentMethod));
                    OnPropertyChanged(nameof(IsOnlinePayment));
                    OnPropertyChanged(nameof(CanShowOnlinePaymentButton));
                    OnPropertyChanged(nameof(PaymentMethodDisplay));
                    OnPropertyChanged(nameof(PaymentStatusText));
                }
            }
        }

        public bool IsOnlinePaymentCompleted
        {
            get => _isOnlinePaymentCompleted;
            set
            {
                if (_isOnlinePaymentCompleted != value)
                {
                    _isOnlinePaymentCompleted = value;
                    OnPropertyChanged(nameof(IsOnlinePaymentCompleted));
                    OnPropertyChanged(nameof(CanShowOnlinePaymentButton));
                    OnPropertyChanged(nameof(CanBeCancelledByClient));
                    OnPropertyChanged(nameof(PaymentStatusText));
                    OnPropertyChanged(nameof(PaymentPaidAtText));
                }
            }
        }

        public System.DateTime? OnlinePaymentPaidAt
        {
            get => _onlinePaymentPaidAt;
            set
            {
                if (_onlinePaymentPaidAt != value)
                {
                    _onlinePaymentPaidAt = value;
                    OnPropertyChanged(nameof(OnlinePaymentPaidAt));
                    OnPropertyChanged(nameof(PaymentPaidAtText));
                }
            }
        }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public OrderStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(CanBeCancelledByClient));
                }
            }
        }

        public System.DateTime CreatedAt { get; set; }
        public System.DateTime? UpdatedAt { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int? ProductId { get; set; }
        public int? ServiceId { get; set; }
        public Product Product { get; set; }
        public Service Service { get; set; }
        public int Quantity { get; set; }
    }

    public enum OrderStatus
    {
        Created,
        Assigned,
        Diagnosing,
        WaitingForParts,
        InProgress,
        ReadyForPickup,
        Completed,
        Cancelled
    }
}
