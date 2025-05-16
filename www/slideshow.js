// File picker and image preview functionality
document.addEventListener('DOMContentLoaded', () => {
    // Load and display a random image from the server
    function loadRandomImage() {
        fetch('/api/GetRandomImage')
            .then(response => response.blob())
            .then(imageBlob => {
                const imageUrl = URL.createObjectURL(imageBlob);
                const imagePreview = document.getElementById('image-preview');
                imagePreview.src = imageUrl;

                // Optional: Release the previous object URL to avoid memory leaks
                const oldUrl = imagePreview.dataset.objectUrl;
                if (oldUrl) {
                    URL.revokeObjectURL(oldUrl);
                }
                
                // Store the new URL for future cleanup
                imagePreview.dataset.objectUrl = imageUrl;
            })
            .catch(error => console.error('Error fetching random image:', error));
    };

    // Initial load
    loadRandomImage();

    // Set interval to load a new random image every 15 seconds
    setInterval(loadRandomImage, 15000);
});