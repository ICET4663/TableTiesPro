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
            // Note: Tailwind doesn't have a direct 'was-validated' class, you'd typically use
            // utility classes like 'invalid:border-red-500' and pseudo-classes like ':invalid'
            // in your CSS or Tailwind configuration to style based on validation state.
            // This class might be left over from a Bootstrap template; you can remove it
            // if you are purely using Tailwind and its validation utilities.
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
            // Assuming the remember me checkbox has the ID 'Input_RememberMe' based on standard Identity UI
            const rememberMeInput = document.getElementById('Input_RememberMe');

            if (!emailInput || !passwordInput || !rememberMeInput) {
                 console.error("Login form inputs not found!");
                 if (loginErrorDiv) {
                      loginErrorDiv.textContent = "Error: Form inputs not found. Please check the HTML IDs.";
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
                    const token = data.token; // Assuming the API returns { token: "..." }
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
                    // Your API returns JSON with a 'Message' property for errors or ModelState errors
                    const errorData = await response.json();
                    let errorMessage = 'An unexpected error occurred during login.'; // Default message

                    if (errorData && errorData.message) { // Assuming API uses 'message' property
                         errorMessage = errorData.message; // Use the message from the API response
                    } else if (errorData && errorData.errors && typeof errorData.errors === 'object') {
                         // Handle validation errors from ModelState
                         let validationErrors = [];
                         for (const field in errorData.errors) {
                              if (errorData.errors.hasOwnProperty(field)) {
                                   validationErrors = validationErrors.concat(errorData.errors[field]);
                              }
                         }
                         if (validationErrors.length > 0) {
                              errorMessage = "Validation errors: " + validationErrors.join(' '); // Join multiple validation messages
                         } else if (errorData && errorData.title) {
                             // Handle other potential error structures (e.g., default API error response)
                              errorMessage = errorData.title;
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
    // Assuming your registration form has the ID 'register-form'
    const registerForm = document.getElementById('register-form');
    const registerErrorDiv = document.querySelector('#register-form [asp-validation-summary="ModelOnly"]'); // Get the validation summary div

    if (registerForm) {
        registerForm.addEventListener('submit', async (event) => {
            // Prevent the default browser form submission
            event.preventDefault();

             // Clear previous error messages
            if (registerErrorDiv) {
                registerErrorDiv.textContent = '';
                registerErrorDiv.style.display = 'none'; // Hide the error div
                registerErrorDiv.style.color = 'red'; // Reset color to red for errors
            }

            // Get input values using the correct IDs from your Register.cshtml
            const emailInput = document.getElementById('Input_Email');
            const passwordInput = document.getElementById('Input_Password');
            const confirmPasswordInput = document.getElementById('Input_ConfirmPassword');

             if (!emailInput || !passwordInput || !confirmPasswordInput) {
                  console.error("Registration form inputs not found!");
                  if (registerErrorDiv) {
                       registerErrorDiv.textContent = "Error: Form inputs not found. Please check the HTML IDs.";
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
                confirmPassword: confirmPassword // Include ConfirmPassword for API validation
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
                    if (data && data.message) { // Assuming API uses 'message' property
                         successMessage = data.message; // Use the message from the API response
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

                    if (errorData && errorData.message) { // Assuming API uses 'message' property
                         // Handle general error messages from the API
                         errorMessage = errorData.message;
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
                              errorMessage = "Validation errors: " + validationErrors.join(' '); // Join multiple validation messages
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


    // --- Forgot Password Form Handling ---
    // Assuming your forgot password form has the ID 'forgot-password-form'
    const forgotPasswordForm = document.getElementById('forgot-password-form');
    // Assuming a div to display messages (success or error) with ID 'forgot-password-message'
    const forgotPasswordMessageDiv = document.getElementById('forgot-password-message');

    if (forgotPasswordForm) {
        forgotPasswordForm.addEventListener('submit', async function (event) {
            event.preventDefault();

            // Clear previous messages
            if (forgotPasswordMessageDiv) {
                forgotPasswordMessageDiv.textContent = '';
                forgotPasswordMessageDiv.style.display = 'none';
                forgotPasswordMessageDiv.style.color = ''; // Reset color
            }

            // Assuming the email input has the ID 'Input_Email' or similar
            const emailInput = document.getElementById('Input_Email'); // Check your HTML for the correct ID

             if (!emailInput) {
                 console.error("Forgot password email input not found!");
                  if (forgotPasswordMessageDiv) {
                       forgotPasswordMessageDiv.textContent = "Error: Email input not found.";
                       forgotPasswordMessageDiv.style.color = 'red';
                       forgotPasswordMessageDiv.style.display = 'block';
                  }
                 return;
             }

            const email = emailInput.value;

            // Basic client-side validation
            if (!email) {
                 if (forgotPasswordMessageDiv) {
                      forgotPasswordMessageDiv.textContent = "Please enter your email address.";
                      forgotPasswordMessageDiv.style.color = 'red';
                      forgotPasswordMessageDiv.style.display = 'block';
                 }
                 return;
            }

            // Create the data payload
            const forgotPasswordData = {
                email: email
            };

            const url = '/api/Account/ForgotPassword'; // Your forgot password API endpoint URL
            const options = {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(forgotPasswordData)
            };

            console.log('Attempting to send password reset link to:', email);

            try {
                 const response = await fetch(url, options);
                 const result = await response.json(); // Assuming API returns JSON

                 if (response.ok) {
                      // API call was successful (status 2xx)
                      console.log("Forgot password request successful:", result);
                       if (forgotPasswordMessageDiv) {
                            forgotPasswordMessageDiv.textContent = result.message || 'Password reset link sent. Please check your email.'; // Display success message from API or default
                            forgotPasswordMessageDiv.style.color = 'green';
                            forgotPasswordMessageDiv.style.display = 'block';
                       } else {
                            alert(result.message || 'Password reset link sent. Please check your email.');
                       }
                 } else {
                      // API call failed (e.g., user not found, invalid email)
                      console.error("Forgot password request failed:", response.status, result);
                       if (forgotPasswordMessageDiv) {
                            forgotPasswordMessageDiv.textContent = result.message || 'Failed to send reset link. Please check the email address.'; // Display error message from API or default
                            forgotPasswordMessageDiv.style.color = 'red';
                            forgotPasswordMessageDiv.style.display = 'block';
                       } else {
                            alert(result.message || 'Failed to send reset link. Please check the email address.');
                       }
                 }

            } catch (error) {
                 console.error("An error occurred during forgot password fetch:", error);
                  if (forgotPasswordMessageDiv) {
                       forgotPasswordMessageDiv.textContent = 'A network error occurred. Please try again.';
                       forgotPasswordMessageDiv.style.color = 'red';
                       forgotPasswordMessageDiv.style.display = 'block';
                  } else {
                       alert('A network error occurred. Please try again.');
                  }
            }
        });
    }

    // --- Logout Functionality ---
    // Assuming you have a logout button or link with the ID 'logout-button'
    const logoutButton = document.getElementById('logout-button');

    if (logoutButton) {
        logoutButton.addEventListener('click', async (event) => {
            event.preventDefault(); // Prevent default link or button behavior

            console.log("Logout initiated.");

            // Remove the token from local storage immediately
            const token = localStorage.getItem('jwtToken');
            if (token) {
                 localStorage.removeItem('jwtToken');
                 console.log("JWT Token removed from localStorage.");
            } else {
                 console.log("No JWT Token found in localStorage to remove.");
            }


            // Optional: Make an API call to the server to invalidate the token server-side
            // This is good practice if your API supports it, but not strictly necessary for JWTs
            // as removing the token client-side prevents access to protected routes.
            // You might still want this to clear server-side session state or logs.
            const logoutUrl = '/api/Account/Logout'; // Replace with your actual logout endpoint if you have one

            // We don't necessarily need to wait for this API call to complete before redirecting,
            // especially if the user just needs to feel like they logged out instantly.
            // If server-side cleanup is critical, you might await this.
            // For a simple JWT setup where client-side token removal is the primary security,
            // making this call fire-and-forget or omitting it is acceptable.
            if (token) { // Only attempt server-side logout if a token was found client-side
                fetch(logoutUrl, {
                     method: 'POST', // Or GET, depending on your API design
                     headers: {
                          'Authorization': `Bearer ${token}` // Include the token in the header
                          // Add Content-Type if your logout endpoint expects a body, but usually it doesn't
                     }
                }).then(response => {
                     if (!response.ok) {
                          console.warn("Server-side logout API call failed with status:", response.status);
                          // Handle server-side logout errors if necessary, but don't block client logout
                     } else {
                         console.log("Server-side logout API call successful.");
                     }
                }).catch(error => {
                     console.error("An error occurred during server-side logout fetch:", error);
                     // Handle network errors for the server-side logout call
                });
            }


            // Redirect to the homepage or logout page after clearing the token
            console.log("Redirecting to homepage after logout.");
            // Redirect to the login page with a logout=true parameter, or the homepage
            window.location.href = '/Account/Login?logout=true'; // Example: Redirect to login with a flag
            // window.location.href = '/'; // Alternative: Redirect to the homepage
        });
    }


    // --- Reset Password Form Handling ---
    // Assuming your reset password form has the ID 'reset-password-form'
    const resetPasswordForm = document.getElementById('reset-password-form');
    // Assuming a div to display messages (success or error) with ID 'reset-password-message'
    const resetPasswordMessageDiv = document.getElementById('reset-password-message');

    if (resetPasswordForm) {
        resetPasswordForm.addEventListener('submit', async function (event) {
            event.preventDefault();

             // Clear previous messages
            if (resetPasswordMessageDiv) {
                resetPasswordMessageDiv.textContent = '';
                resetPasswordMessageDiv.style.display = 'none';
                resetPasswordMessageDiv.style.color = ''; // Reset color
            }

            // Get input values using the correct IDs from your ResetPassword.cshtml
            // Ensure these IDs match your HTML form elements
            const emailInput = document.getElementById('Input_Email');
            const tokenInput = document.getElementById('Input_Token'); // The hidden input for the token
            const newPasswordInput = document.getElementById('Input_NewPassword');
            const confirmPasswordInput = document.getElementById('Input_ConfirmPassword');

             if (!emailInput || !tokenInput || !newPasswordInput || !confirmPasswordInput) {
                  console.error("Reset password form inputs not found!");
                   if (resetPasswordMessageDiv) {
                        resetPasswordMessageDiv.textContent = "Error: Form inputs not found. Please check the HTML IDs.";
                        resetPasswordMessageDiv.style.color = 'red';
                        resetPasswordMessageDiv.style.display = 'block';
                   }
                  return;
             }

            const email = emailInput.value;
            const token = tokenInput.value;
            const newPassword = newPasswordInput.value;
            const confirmPassword = confirmPasswordInput.value;

            // Create the data payload
            const resetPasswordData = {
                email: email,
                token: token,
                newPassword: newPassword,
                confirmPassword: confirmPassword
            };

            const url = '/api/Account/ResetPassword'; // Your reset password API endpoint URL
            const options = {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(resetPasswordData)
            };

            console.log('Attempting to reset password for:', email);

            try {
                 const response = await fetch(url, options);
                 const result = await response.json(); // Assuming API returns JSON

                 if (response.ok) {
                      // API call was successful (status 2xx)
                      console.log("Password reset successful:", result);
                       if (resetPasswordMessageDiv) {
                            resetPasswordMessageDiv.textContent = result.message || 'Your password has been reset successfully.'; // Display success message from API or default
                            resetPasswordMessageDiv.style.color = 'green';
                            resetPasswordMessageDiv.style.display = 'block';

                            // Optionally clear the form fields on success
                            // newPasswordInput.value = '';
                            // confirmPasswordInput.value = '';

                            // Optionally redirect to the login page after a delay
                            // setTimeout(() => { window.location.href = '/Account/Login'; }, 3000); // Redirect after 3 seconds
                       } else {
                            alert(result.message || 'Your password has been reset successfully.');
                       }
                 } else {
                      // API call failed (e.g., invalid token, weak password)
                      console.error("Password reset failed:", response.status, result);
                      let errorMessage = result.message || 'Failed to reset password.';
                       if (result.errors && typeof result.errors === 'object') {
                            // Handle validation errors from ModelState
                            let validationErrors = [];
                            for (const field in result.errors) {
                                if (result.errors.hasOwnProperty(field)) {
                                     const fieldName = field === '' ? '' : `${field.replace('Input.', '').replace('.', ' ')}: `;
                                     validationErrors = validationErrors.concat(result.errors[field].map(msg => `${fieldName}${msg}`));
                                }
                            }
                            if (validationErrors.length > 0) {
                                 errorMessage = "Validation errors: " + validationErrors.join(' ');
                             } else if (result.title) {
                                 errorMessage = result.title;
                             }
                       }


                       if (resetPasswordMessageDiv) {
                            resetPasswordMessageDiv.textContent = errorMessage;
                            resetPasswordMessageDiv.style.color = 'red';
                            resetPasswordMessageDiv.style.display = 'block';
                       } else {
                            alert(errorMessage);
                       }
                 }

            } catch (error) {
                 console.error("An error occurred during reset password fetch:", error);
                  if (resetPasswordMessageDiv) {
                       resetPasswordMessageDiv.textContent = 'A network error occurred. Please try again.';
                       resetPasswordMessageDiv.style.color = 'red';
                       resetPasswordMessageDiv.style.display = 'block';
                  } else {
                       alert('A network error occurred. Please try again.');
                  }
            }
        });
    }


    // Add other account-related functionality here as needed
    // For example, handling email confirmation links, profile editing forms, etc.
});

