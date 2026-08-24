document.addEventListener('DOMContentLoaded', function () {
    const bookingForm = document.getElementById('restaurant-booking-form') || document.getElementById('hotel-booking-form');
    
    if (bookingForm) {
        bookingForm.addEventListener('submit', function (event) {
            event.preventDefault();

            // Gather form data
            const formData = new FormData(bookingForm);
            const data = {};
            formData.forEach((value, key) => {
                data[key] = value;
            });

            // Make an API call or update the UI accordingly
            console.log('Booking Data:', data);

            // Example: You can send the data to a server here
            // fetch('/api/Booking/CreateBooking', {
            //    method: 'POST',
            //    headers: {
            //        'Content-Type': 'application/json'
            //    },
            //    body: JSON.stringify(data)
            // }).then(response => response.json()).then(result => {
            //    console.log(result);
            // });
        });
    }
});
