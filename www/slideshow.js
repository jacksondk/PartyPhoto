// File picker and image preview functionality
document.addEventListener('DOMContentLoaded', () => {
    // Load and display a random image from the server
    var activeImageIndex = 1;

    function loadRandomImage() {
        var nextImageElement = document.getElementById('image-' + activeImageIndex);
        var currentImageElement = document.getElementById('image-' + (activeImageIndex === 1 ? 2 : 1));
        activeImageIndex = activeImageIndex === 1 ? 2 : 1;

        fetch('/api/GetRandomImage')
            .then(response => response.blob())
            .then(imageBlob => {
                const imageUrl = URL.createObjectURL(imageBlob);
                nextImageElement.src = imageUrl;

                // Store the new URL for future cleanup
                const oldUrl = nextImageElement.dataset.objectUrl;
                if (oldUrl) {
                    URL.revokeObjectURL(oldUrl);
                }
                nextImageElement.dataset.objectUrl = imageUrl;

                // Once image is loaded, perform the crossfade
                nextImageElement.onload = function () {
                    // Switch active class
                    currentImageElement.classList.remove('active');
                    nextImageElement.classList.add('active');

                    // Swap active index for next iteration
                    activeImageIndex = activeImageIndex === 1 ? 2 : 1;
                };
            })
            .catch(error => console.error('Error fetching random image:', error));
    };

    // Initial load
    loadRandomImage();

    // Set interval to load a new random image every 15 seconds
    setInterval(loadRandomImage, 15000);
});