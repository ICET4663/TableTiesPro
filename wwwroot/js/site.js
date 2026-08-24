// Ensure the DOM is fully loaded before running scripts
document.addEventListener('DOMContentLoaded', function () {

    // --- Registration Functionality ---
    // Get the registration form and button (adjust IDs as needed)
    const registerForm = document.getElementById('registerForm');
    const registerButton = document.getElementById('registerButton'); // Assuming a button with this ID

    // Add an event listener to the registration button or form submission
    if (registerForm) {
        registerForm.addEventListener('submit', handleRegister);
    } else if (registerButton) {
        registerButton.addEventListener('click', handleRegister);
    }

    /**
     * Handles the registration process when the form is submitted or button is clicked.
     * @param {Event} event - The DOM event (e.g., submit or click).
     */
    async function handleRegister(event) {
        // Prevent the default form submission if it's a submit event
        if (event.type === 'submit') {
            event.preventDefault();
        }

        // Get form data (adjust input field IDs or names as needed)
        const emailInput = document.getElementById('registerEmail'); // Assuming input with ID 'registerEmail'
        const passwordInput = document.getElementById('registerPassword'); // Assuming input with ID 'registerPassword'
        const confirmPasswordInput = document.getElementById('registerConfirmPassword'); // Assuming input with ID 'registerConfirmPassword'
        const errorMessageElement = document.getElementById('registerErrorMessage'); // Assuming an element to display errors

        // Basic client-side validation
        if (!emailInput || !passwordInput || !confirmPasswordInput) {
            console.error('Registration form inputs not found.');
            if (errorMessageElement) {
                 errorMessageElement.textContent = 'An internal error occurred. Please try again later.';
            }
            return;
        }

        const email = emailInput.value;
        const password = passwordInput.value;
        const confirmPassword = confirmPasswordInput.value;

        if (password !== confirmPassword) {
            if (errorMessageElement) {
                errorMessageElement.textContent = 'Passwords do not match.';
            }
            return;
        }

        // Clear previous error messages
        if (errorMessageElement) {
            errorMessageElement.textContent = '';
        }

        // Prepare data to send to the API
        const registrationData = {
            email: email,
            password: password,
            confirmPassword: confirmPassword
            // Add other registration fields if needed
        };

        try {
            // Send registration data to your backend API (adjust URL as needed)
            const response = await fetch('/api/account/register', { // Assuming your API endpoint is /api/account/register
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(registrationData),
            });

            if (response.ok) {
                // Registration successful
                console.log('Registration successful!');
                // Redirect the user or show a success message
                window.location.href = '/login'; // Example: Redirect to login page
            } else {
                // Registration failed
                const errorData = await response.json(); // Assuming API returns JSON errors
                console.error('Registration failed:', response.status, errorData);
                if (errorMessageElement) {
                    // Display error message from the API or a generic one
                    errorMessageElement.textContent = errorData.message || 'Registration failed. Please check your details.';
                }
            }
        } catch (error) {
            // Handle network errors or other exceptions
            console.error('An error occurred during registration:', error);
            if (errorMessageElement) {
                 errorMessageElement.textContent = 'An unexpected error occurred. Please try again later.';
            }
        }
    }


    // --- Login Functionality ---
    // Get the login form and button (adjust IDs as needed)
    const loginForm = document.getElementById('loginForm');
    const loginButton = document.getElementById('loginButton'); // Assuming a button with this ID

    // Add an event listener to the login button or form submission
     if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    } else if (loginButton) {
        loginButton.addEventListener('click', handleLogin);
    }


    /**
     * Handles the login process when the form is submitted or button is clicked.
     * @param {Event} event - The DOM event (e.g., submit or click).
     */
    async function handleLogin(event) {
         // Prevent the default form submission if it's a submit event
        if (event.type === 'submit') {
            event.preventDefault();
        }

        // Get form data (adjust input field IDs or names as needed)
        const emailInput = document.getElementById('loginEmail'); // Assuming input with ID 'loginEmail'
        const passwordInput = document.getElementById('loginPassword'); // Assuming input with ID 'loginPassword'
        const errorMessageElement = document.getElementById('loginErrorMessage'); // Assuming an element to display errors

         if (!emailInput || !passwordInput) {
            console.error('Login form inputs not found.');
             if (errorMessageElement) {
                 errorMessageElement.textContent = 'An internal error occurred. Please try again later.';
            }
            return;
        }

        const email = emailInput.value;
        const password = passwordInput.value;

        // Clear previous error messages
        if (errorMessageElement) {
            errorMessageElement.textContent = '';
        }

        // Prepare data to send to the API
        const loginData = {
            email: email,
            password: password
        };

        try {
            // Send login data to your backend API (adjust URL as needed)
            const response = await fetch('/api/account/login', { // Assuming your API endpoint is /api/account/login
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(loginData),
            });

            if (response.ok) {
                // Login successful
                console.log('Login successful!');
                // Handle successful login (e.g., store token, redirect)
                // If your API returns a JWT token, you might store it in localStorage or cookies
                // const token = await response.text(); // Or response.json() if token is in a JSON object
                // localStorage.setItem('jwtToken', token);

                window.location.href = '/'; // Example: Redirect to the home page
            } else {
                // Login failed
                const errorData = await response.json(); // Assuming API returns JSON errors
                console.error('Login failed:', response.status, errorData);
                 if (errorMessageElement) {
                    // Display error message from the API or a generic one
                    errorMessageElement.textContent = errorData.message || 'Login failed. Please check your credentials.';
                }
            }
        } catch (error) {
            // Handle network errors or other exceptions
            console.error('An error occurred during login:', error);
             if (errorMessageElement) {
                 errorMessageElement.textContent = 'An unexpected error occurred. Please try again later.';
            }
        }
    }

    // --- Other potential client-side JS ---

    // Example: Function to fetch data from an API endpoint (like getting restaurants)
    async function fetchRestaurants() {
        try {
            const response = await fetch('/api/restaurants'); // Assuming API endpoint is /api/restaurants
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const restaurants = await response.json();
            console.log('Fetched restaurants:', restaurants);
            // Process and display the restaurants data
            return restaurants;
        } catch (error) {
            console.error('Error fetching restaurants:', error);
            // Handle the error (e.g., display a message to the user)
            return null;
        }
    }

    // Example usage of fetchRestaurants (e.g., on a page load)
    // fetchRestaurants();

    // Add more client-side JavaScript functions as needed for your application

}); // End of DOMContentLoaded listener
