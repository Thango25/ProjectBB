$(document).ready(function () {
    // --- Common Functions (Existing or new) ---

    // Example for your existing "Upload Lost Item Modal" (if it uses similar logic)
    // You might already have some JS here, if so, integrate this new part.
    // Assuming your "Upload Lost Item Modal" is static for now.

    // --- Profile Modal Specific Functions ---
    const profileModal = $('#profileModal');
    const profileNavLink = $('#profileNavLink');
    const profileModalBodyContent = $('#profileModalBodyContent');
    const editProfileBtn = $('#editProfileBtn');
    const saveProfileBtn = $('#saveProfileBtn');

    // Function to load profile content into the modal body via AJAX
    function loadProfileContent() {
        $.get('/Profile/GetProfileData', function (data) {
            profileModalBodyContent.html(data);
            // Re-initialize client-side validation for the new form
            var form = $('#profileForm');
            form.removeData('validator');
            form.removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse(form);

            // After content is loaded, set the initial mode to display
            toggleProfileMode(false);
        }).fail(function () {
            profileModalBodyContent.html('<div class="text-danger">Failed to load profile. Please try again.</div>');
        });
    }

    // Function to toggle between display and edit modes
    function toggleProfileMode(isEditing) {
        // These elements are inside the dynamically loaded partial view,
        // so we select them within the modal's content
        const profileDisplay = profileModalBodyContent.find('#profileDisplay');
        const profileEdit = profileModalBodyContent.find('#profileEdit');
        const userProfileAvatar = profileModalBodyContent.find('#userProfileAvatar');
        const avatarUpload = profileModalBodyContent.find('#avatarUpload');

        if (isEditing) {
            profileDisplay.hide();
            profileEdit.show();
            editProfileBtn.hide();
            saveProfileBtn.show();

            // Populate edit fields with current display values
            profileModalBodyContent.find('#Username').val(profileModalBodyContent.find('#displayUsername').text());
            profileModalBodyContent.find('#Email').val(profileModalBodyContent.find('#displayEmail').text());
            profileModalBodyContent.find('#PhoneNumber').val(profileModalBodyContent.find('#displayPhone').text());
            profileModalBodyContent.find('#Location').val(profileModalBodyContent.find('#displayLocation').text());

        } else {
            profileDisplay.show();
            profileEdit.hide();
            editProfileBtn.show();
            saveProfileBtn.hide();
        }

        // Attach avatar upload preview listener (needs to be re-attached if content is dynamic)
        avatarUpload.off('change').on('change', function (event) {
            const file = event.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    userProfileAvatar.attr('src', e.target.result);
                };
                reader.readAsDataURL(file);
            }
        });
    }

    // --- Event Handlers ---

    // When the "Profile" nav link is clicked, load the content and then show the modal
    profileNavLink.on('click', function (e) {
        e.preventDefault(); // Prevent default link behavior
        loadProfileContent(); // Load content first
        profileModal.modal('show'); // Then show the modal
    });

    // Handle Edit button click
    editProfileBtn.on('click', function () {
        toggleProfileMode(true);
    });

    // Handle Save button click
    saveProfileBtn.on('click', function (e) {
        e.preventDefault(); // Prevent default button behavior
        var form = profileModalBodyContent.find('#profileForm'); // Select the form inside the modal

        // Client-side validation check
        if (!form.valid()) {
            return; // Stop if form is not valid
        }

        var formData = new FormData(form[0]); // Create FormData object for file upload

        // Append avatar file if selected
        var avatarFile = profileModalBodyContent.find('#avatarUpload')[0].files[0];
        if (avatarFile) {
            formData.append('AvatarFile', avatarFile);
        }

        $.ajax({
            url: '/Profile/SaveProfile', // MVC action URL
            type: 'POST',
            data: formData,
            processData: false, // Important for FormData
            contentType: false, // Important for FormData
            success: function (response) {
                if (response.success) {
                    alert(response.message); // Show success message

                    // Update the display values from the response
                    profileModalBodyContent.find('#displayUsername').text(response.profile.username);
                    profileModalBodyContent.find('#displayEmail').text(response.profile.email);
                    profileModalBodyContent.find('#displayPhone').text(response.profile.phoneNumber);
                    profileModalBodyContent.find('#displayLocation').text(response.profile.location);
                    if (response.profile.avatarUrl) {
                        profileModalBodyContent.find('#userProfileAvatar').attr('src', response.profile.avatarUrl);
                    }

                    toggleProfileMode(false); // Switch back to display mode
                } else {
                    alert('Error: ' + response.message + '\n' + (response.errors ? response.errors.join('\n') : ''));
                }
            },
            error: function (xhr, status, error) {
                alert('An error occurred while saving profile: ' + xhr.responseText);
            }
        });
    });
});

//**************************** Report - category ****************************

    
        document.addEventListener('DOMContentLoaded', function () {
            // Data for the chart from the Model
            const categoryData = @Html.Raw(JsonSerializer.Serialize(Model.CategoryItemCounts));

            const labels = categoryData.map(item => item.categoryName);
            const counts = categoryData.map(item => item.itemCount);

            const ctx = document.getElementById('categoryChart').getContext('2d');
            new Chart(ctx, {
                type: 'bar', // You can change this to 'pie', 'doughnut', etc.
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Number of Items',
                        data: counts,
                        backgroundColor: [
                            'rgba(255, 99, 132, 0.6)', 'rgba(54, 162, 235, 0.6)', 'rgba(255, 206, 86, 0.6)',
                            'rgba(75, 192, 192, 0.6)', 'rgba(153, 102, 255, 0.6)', 'rgba(255, 159, 64, 0.6)',
                            'rgba(192, 192, 192, 0.6)', 'rgba(128, 0, 128, 0.6)', 'rgba(0, 128, 0, 0.6)',
                            'rgba(0, 0, 128, 0.6)', 'rgba(128, 128, 0, 0.6)', 'rgba(0, 128, 128, 0.6)'
                        ],
                        borderColor: [
                            'rgba(255, 99, 132, 1)', 'rgba(54, 162, 235, 1)', 'rgba(255, 206, 86, 1)',
                            'rgba(75, 192, 192, 1)', 'rgba(153, 102, 255, 1)', 'rgba(255, 159, 64, 1)',
                            'rgba(192, 192, 192, 1)', 'rgba(128, 0, 128, 1)', 'rgba(0, 128, 0, 1)',
                            'rgba(0, 0, 128, 1)', 'rgba(128, 128, 0, 1)', 'rgba(0, 128, 128, 1)'
                        ],
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true,
                            title: {
                                display: true,
                                text: 'Number of Items'
                            }
                        },
                        x: {
                            title: {
                                display: true,
                                text: 'Category'
                            }
                        }
                    },
                    plugins: {
                        legend: {
                            display: false // Hide dataset legend if only one dataset
                        },
                        title: {
                            display: true,
                            text: 'Number of Items per Category'
                        }
                    }
                }
            });
        });

document.addEventListener('DOMContentLoaded', function () {
    fetch('/Items/GetCategoryItemCounts')
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            if (data && data.length > 0) {
                const labels = data.map(item => item.categoryName);
                const counts = data.map(item => item.itemCount);

                const ctx = document.getElementById('categoryChart').getContext('2d');
                new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Number of Items',
                            data: counts,
                            backgroundColor: [
                                'rgba(255, 99, 132, 0.6)',
                                'rgba(54, 162, 235, 0.6)',
                                'rgba(255, 206, 86, 0.6)',
                                'rgba(75, 192, 192, 0.6)',
                                'rgba(153, 102, 255, 0.6)',
                                'rgba(255, 159, 64, 0.6)'
                            ],
                            borderColor: [
                                'rgba(255, 99, 132, 1)',
                                'rgba(54, 162, 235, 1)',
                                'rgba(255, 206, 86, 1)',
                                'rgba(75, 192, 192, 1)',
                                'rgba(153, 102, 255, 1)',
                                'rgba(255, 159, 64, 1)'
                            ],
                            borderWidth: 1
                        }]
                    },
                    options: {
                        responsive: true,
                        scales: {
                            y: {
                                beginAtZero: true
                            }
                        }
                    }
                });
            } else {
                console.log('No category data received.');
            }
        })
        .catch(error => console.error('Error fetching category data:', error));
});
//**************************** End of Report - category ****************************
    