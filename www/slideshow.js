// File picker and image preview functionality
document.addEventListener('DOMContentLoaded', () => {
    // Load and display a random image from the server    
    var activeImageIndex = 1;
    function loadImage() {
        var nextImageElement = document.getElementById('image-' + activeImageIndex);
        var currentImageElement = document.getElementById('image-' + (activeImageIndex === 1 ? 2 : 1));
        activeImageIndex = activeImageIndex === 1 ? 2 : 1;

        fetch('/api/RandomImage')
            .then(response => response.blob())
            .then(imageBlob => {
                const imageUrl = URL.createObjectURL(imageBlob);
                // Load new image in the inactive element
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
    }
    loadImage();
    setInterval(loadImage, 5000);
});