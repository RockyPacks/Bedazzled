using System;

namespace Bedazzled.Client.Services
{
    public class ModalService
    {
        public bool IsBookingModalVisible { get; private set; }
        public event Action? OnModalStateChanged;

        public void ShowBookingModal()
        {
            IsBookingModalVisible = true;
            OnModalStateChanged?.Invoke();
        }

        public void HideBookingModal()
        {
            IsBookingModalVisible = false;
            OnModalStateChanged?.Invoke();
        }
    }
}
