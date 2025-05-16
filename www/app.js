// File picker and image preview functionality
document.addEventListener('DOMContentLoaded', () => {
  const fileInput = document.getElementById('image-upload');
  const liveInput = document.getElementById('image-live');
  

  const imagePreviewContainer = document.getElementById('image-preview-container');
  const imagePreview = document.getElementById('image-preview');
  const uploadBtn = document.getElementById('upload-btn');

  // Create upload progress elements
  const progressContainer = document.createElement('div');
  progressContainer.className = 'progress-container hidden';

  const progressBar = document.createElement('div');
  progressBar.className = 'progress-bar';

  const progressText = document.createElement('div');
  progressText.className = 'progress-text';
  progressText.textContent = '0%';

  progressContainer.appendChild(progressBar);
  progressContainer.appendChild(progressText);

  // Insert after preview container
  imagePreviewContainer.parentNode.insertBefore(progressContainer, imagePreviewContainer.nextSibling);


  // Listen for file selection
  fileInput.addEventListener('change', (event) => {
    const file = event.target.files[0];

    if (file) {
      // Show the preview container
      imagePreviewContainer.classList.remove('hidden');

      // Create a URL for the selected image
      const imageUrl = URL.createObjectURL(file);

      // Set the preview image source
      imagePreview.src = imageUrl;

      // Log file information
      console.log('Selected file:', file.name, 'Size:', (file.size / 1024).toFixed(2), 'KB');
    }
  });  // Handle upload button click

  liveInput.addEventListener('change', (event) => {
    const file = event.target.files[0];

    if (file) {
      // Show the preview container
      imagePreviewContainer.classList.remove('hidden');

      // Create a URL for the selected image
      const imageUrl = URL.createObjectURL(file);

      // Set the preview image source
      imagePreview.src = imageUrl;

      // Log file information
      console.log('Selected file:', file.name, 'Size:', (file.size / 1024).toFixed(2), 'KB');
    }
  });

  uploadBtn.addEventListener('click', () => {
    // Check both file inputs and use whichever has a file
    const fileFromPicker = fileInput.files[0];
    const fileFromCamera = liveInput.files[0];
    const file = fileFromPicker || fileFromCamera;

    if (file) {
      // Show loading state
      uploadBtn.textContent = 'Uploading...';
      uploadBtn.disabled = true;

      // Show and reset progress bar
      progressContainer.classList.remove('hidden');
      progressBar.style.width = '0%';
      progressText.textContent = '0%';

      const formData = new FormData();
      formData.append('file', file);

      // Create XHR request to track progress
      const xhr = new XMLHttpRequest();

      xhr.upload.addEventListener('progress', (event) => {
        if (event.lengthComputable) {
          const percentage = Math.round((event.loaded / event.total) * 100);
          progressBar.style.width = percentage + '%';
          progressText.textContent = percentage + '%';
        }
      });

      xhr.addEventListener('load', function () {
        if (xhr.status >= 200 && xhr.status < 300) {
          const response = JSON.parse(xhr.responseText);
          console.log('Success:', response);

          // Show success message
          const successMessage = document.createElement('div');
          successMessage.className = 'success-message';
          successMessage.textContent = `Image uploaded successfully!`;
          progressContainer.parentNode.insertBefore(successMessage, progressContainer.nextSibling);

          // Auto-remove message after 5 seconds
          setTimeout(() => {
            successMessage.remove();
          }, 5000);
        } else {
          console.error('Error:', xhr.statusText);
          alert(`Error uploading file: ${xhr.statusText}`);
        }
      });

      xhr.addEventListener('error', function () {
        console.error('Network error occurred');
        alert('Network error occurred during upload');
      });

      xhr.addEventListener('loadend', function () {
        // Reset UI state
        uploadBtn.textContent = 'Upload Image';
        uploadBtn.disabled = false;

        // Hide progress after 1 second
        setTimeout(() => {
          progressContainer.classList.add('hidden');
        }, 1000);
      });

      xhr.open('POST', '/api/Upload', true);
      xhr.send(formData);
    } else {
      alert('Please select an image first');
    }
  });
});
