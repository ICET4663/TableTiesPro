// account.js

// Wait for the DOM to be fully loaded before running the script
document.addEventListener('DOMContentLoaded', function () {
    'use strict'; // Enforce stricter parsing and error handling

    // --- Client-side form validation ---
    // This handles the basic HTML5 validation and adds Bootstrap's 'was-validated' class
    // For more robust validation, especially after API calls, we'll handle server errors below.
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                // If the form is invalid based on HTML5 validation, prevent submission
                event.preventDefault();
                event.stopPropagation();
            }
            // Add 'was-validated' class to show validation feedback (using Bootstrap/Tailwind styles)
            form.classList.add('was-validated');
        }, false);
    });

    // --- Login Form Handling ---
    const loginForm = document.getElementById('login-form');
    const loginErrorDiv = document.querySelector('#login-form [asp-validation-summary="ModelOnly"]'); // Get the validation summary div

    if (loginForm) {
        loginForm.addEventListener('submit', async (event) => {
            // Prevent the default browser form submission
            event.preventDefault();

            // Clear previous error messages
            if (loginErrorDiv) {
                loginErrorDiv.textContent = '';
                loginErrorDiv.style.display = 'none'; // Hide the error div
            }

            // Get input values using the correct IDs from your Login.cshtml
            const emailInput = document.getElementById('Input_Email');
            const passwordInput = document.getElementById('Input_Password');
            const rememberMeInput = document.getElementById('rememberMe'); // Assuming this ID is correct

            if (!emailInput || !passwordInput || !rememberMeInput) {
                 console.error("Login form inputs not found!");
                 if (loginErrorDiv) {
                      loginErrorDiv.textContent = "Error: Form inputs not found.";
                      loginErrorDiv.style.display = 'block';
                 }
                 return; // Stop if inputs aren't found
            }

            const email = emailInput.value;
            const password = passwordInput.value;
            const rememberMe = rememberMeInput.checked;


            // Create the data payload matching your C# LoginModel DTO
            const loginData = {
                email: email,
                password: password,
                rememberMe: rememberMe
            };

            const url = '/api/Account/Login'; // Your login API endpoint URL
            const options = {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json' // Specify content type as JSON
                },
                body: JSON.stringify(loginData) // Convert data object to JSON string
            };

            try {
                // Make the API call using the fetch API
                const response = await fetch(url, options);

                // Check if the response was successful (status code 2xx)
                if (response.ok) {
                    // Login successful!
                    console.log("Login successful!");
                    const data = await response.json(); // Parse the JSON response body

                    // Extract and store the JWT token
                    const token = data.Token;
                    if (token) {
                        localStorage.setItem('jwtToken', token); // Store the token in local storage
                        console.log("JWT Token stored:", token);

                        // Redirect the user
                        // Check for a ReturnUrl query parameter first
                        const params = new URLSearchParams(window.location.search);
                        const returnUrl = params.get('ReturnUrl');

                        if (returnUrl) {
                            console.log(`Redirecting to ReturnUrl: ${returnUrl}`);
                            window.location.href = returnUrl; // Redirect to the original page
                        } else {
                            // Default redirect if no ReturnUrl is present
                            console.log("No ReturnUrl, redirecting to default page /Book/Restaurant");
                            window.location.href = '/Book/Restaurant'; // Your desired default page
                        }

                    } else {
                         // Token not found in successful response (shouldn't happen if API is correct)
                         console.error("Login successful but no token received.");
                         if (loginErrorDiv) {
                              loginErrorDiv.textContent = "Login successful, but failed to get token. Please try again.";
                              loginErrorDiv.style.display = 'block';
                         }
                    }

                } else {
                    // Login failed (HTTP status code is not 2xx)
                    console.error("Login failed with status:", response.status);

                    // Attempt to read the error message from the response body
                    // Your API returns JSON with a 'Message' property for errors
                    const errorData = await response.json();
                    let errorMessage = 'An unexpected error occurred during login.'; // Default message

                    if (errorData && errorData.Message) {
                        errorMessage = errorData.Message; // Use the message from the API response
                    } else if (errorData && typeof errorData === 'object') {
                         // Handle validation errors from ModelState (less common for login, but possible)
                         // The structure might be { errors: { "Field.Name": ["Error message"] } }
                         let validationErrors = [];
                         for (const field in errorData.errors) {
                             if (errorData.errors.hasOwnProperty(field)) {
                                 validationErrors = validationErrors.concat(errorData.errors[field]);
                             }
                         }
                         if (validationErrors.length > 0) {
                              errorMessage = validationErrors.join(' '); // Join multiple validation messages
                         }
                    }


                    // Display the error message to the user
                    if (loginErrorDiv) {
                        loginErrorDiv.textContent = errorMessage;
                        loginErrorDiv.style.display = 'block'; // Show the error div
                    } else {
                         // Fallback if the error div is not found
                         alert(errorMessage);
                    }
                }

            } catch (error) {
                // Handle network errors or other exceptions during the fetch request
                console.error("An error occurred during login fetch:", error);
                if (loginErrorDiv) {
                    loginErrorDiv.textContent = 'A network error occurred. Please try again.';
                    loginErrorDiv.style.display = 'block'; // Show the error div
                } else {
                     alert('A network error occurred. Please try again.');
                }
            }
        });
    }

    // --- Registration Form Handling ---
    const registerForm = document.getElementById('registerForm'); // Assuming this is the ID of your registration form
     const registerErrorDiv = document.querySelector('#registerForm [asp-validation-summary="ModelOnly"]'); // Get the validation summary div

    if (registerForm) {
        registerForm.addEventListener('submit', async (event) => {
            // Prevent the default browser form submission
            event.preventDefault();

             // Clear previous error messages
            if (registerErrorDiv) {
                registerErrorDiv.textContent = '';
                registerErrorDiv.style.display = 'none'; // Hide the error div
            }

            // Get input values using the correct IDs from your Register.cshtml
            const emailInput = document.getElementById('Input_Email');
            const passwordInput = document.getElementById('Input_Password');
            const confirmPasswordInput = document.getElementById('Input_ConfirmPassword');

             if (!emailInput || !passwordInput || !confirmPasswordInput) {
                 console.error("Registration form inputs not found!");
                  if (registerErrorDiv) {
                      registerErrorDiv.textContent = "Error: Form inputs not found.";
                      registerErrorDiv.style.display = 'block';
                 }
                 return; // Stop if inputs aren't found
            }

            const email = emailInput.value;
            const password = passwordInput.value;
            const confirmPassword = confirmPasswordInput.value;


            // Create the data payload matching your C# RegisterModel DTO
            const registerData = {
                email: email,
                password: password,
                confirmPassword: confirmPassword // Include ConfirmPassword for client-side validation if needed
            };

            const url = '/api/Account/Register'; // Your registration API endpoint URL
            const options = {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json' // Specify content type as JSON
                },
                body: JSON.stringify(registerData) // Convert data object to JSON string
            };

            try {
                // Make the API call using the fetch API
                const response = await fetch(url, options);

                // Check if the response was successful (status code 2xx)
                if (response.ok) {
                    // Registration successful!
                    console.log("Registration successful!");
                    const data = await response.json(); // Parse the JSON response body

                    // Display the success message from the API
                    let successMessage = "Registration successful!";
                    if (data && data.Message) {
                         successMessage = data.Message; // Use the message from the API response
                    }

                    // Display success message (e.g., in the error div area temporarily)
                    if (registerErrorDiv) { // Re-using the error div area for success message
                         registerErrorDiv.textContent = successMessage;
                         registerErrorDiv.style.color = 'green'; // Change color for success
                         registerErrorDiv.style.display = 'block';
                    } else {
                         alert(successMessage);
                    }

                    // Optionally, redirect to the login page after successful registration
                    // window.location.href = '/Account/Login';

                } else {
                    // Registration failed (HTTP status code is not 2xx, likely 400 Bad Request)
                    console.error("Registration failed with status:", response.status);

                    // Attempt to read the error message(s) from the response body
                    // Your API returns JSON with ModelState errors or a general Message
                    const errorData = await response.json();
                    let errorMessage = 'An unexpected error occurred during registration.'; // Default message
                    let validationErrors = [];

                    if (errorData && errorData.Message) {
                        // Handle general error messages from the API
                        errorMessage = errorData.Message;
                    } else if (errorData && errorData.errors && typeof errorData.errors === 'object') {
                        // Handle validation errors from ModelState
                        // The structure is typically { errors: { "Field.Name": ["Error message"] } }
                        for (const field in errorData.errors) {
                            if (errorData.errors.hasOwnProperty(field)) {
                                // Concatenate field name (if applicable) and error messages
                                const fieldName = field === '' ? '' : `${field.replace('Input.', '').replace('.', ' ')}: `; // Clean up field name for display
                                validationErrors = validationErrors.concat(errorData.errors[field].map(msg => `${fieldName}${msg}`));
                            }
                        }
                        if (validationErrors.length > 0) {
                             errorMessage = validationErrors.join(' '); // Join multiple validation messages
                        } else if (errorData && errorData.title) {
                            // Handle other potential error structures (e.g., default API error response)
                            errorMessage = errorData.title;
                        }
                    }


                    // Display the error message(s) to the user
                    if (registerErrorDiv) {
                        registerErrorDiv.textContent = errorMessage;
                        registerErrorDiv.style.color = 'red'; // Ensure error color
                        registerErrorDiv.style.display = 'block'; // Show the error div
                    } else {
                         // Fallback if the error div is not found
                         alert(errorMessage);
                    }
                }

            } catch (error) {
                // Handle network errors or other exceptions during the fetch request
                console.error("An error occurred during registration fetch:", error);
                if (registerErrorDiv) {
                    registerErrorDiv.textContent = 'A network error occurred. Please try again.';
                    registerErrorDiv.style.color = 'red'; // Ensure error color
                    registerErrorDiv.style.display = 'block'; // Show the error div
                } else {
                     alert('A network error occurred. Please try again.');
                }
            }
        });
    }


    // Handle password reset request (existing code)
    const forgotPasswordForm = document.querySelector('#forgot-password-form');
    if (forgotPasswordForm) {
        forgotPasswordForm.addEventListener('submit', function (event) {
            event.preventDefault();

            const email = document.querySelector('#reset-email').value;

            // API call for sending reset password email
            console.log('Sending password reset link to:', email);

            // TODO: Implement actual API call for password reset
        });
    }

    // Add other account-related functionality here
});
