document.addEventListener('DOMContentLoaded', function () {
    const dateInput = document.querySelector('#date');
    const timeInput = document.querySelector('#time');
    const partySizeInput = document.querySelector('#party-size');

    // Example: Handle date selection change to update available times
    if (dateInput) {
        dateInput.addEventListener('change', function () {
            const selectedDate = dateInput.value;

            // You can update available times based on the selected date
            console.log('Selected Date:', selectedDate);
            // For example, make an API call to fetch available times for this date
            // fetch(`/api/Booking/GetAvailableTimes?date=${selectedDate}`)
            //     .then(response => response.json())
            //     .then(times => {
            //         // Update the time input with available options
            //         times.forEach(time => {
            //             const option = document.createElement('option');
            //             option.value = time;
            //             option.textContent = time;
            //             timeInput.appendChild(option);
            //         });
            //     });
        });
    }
});
