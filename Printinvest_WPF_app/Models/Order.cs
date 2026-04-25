using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Printinvest_WPF_app.Models
{
    public class Order : INotifyPropertyChanged
    {
        private OrderStatus _status;
        private byte[] _problemPhoto;
        private bool _isDetailsExpanded;

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
        public string DetailsButtonText => IsDetailsExpanded ? "Скрыть" : "Подробнее";
        public string ContactPhone { get; set; }
        public string ClientComment { get; set; }
        public string AdminComment { get; set; }
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
                }
            }
        }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

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
