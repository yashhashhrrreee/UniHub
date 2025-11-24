// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

// Toggle show/hide for degree lists used on Product Read page
document.addEventListener('DOMContentLoaded', function () {
	function setupToggle(btnId, containerId) {
		var btn = document.getElementById(btnId);
		var container = document.getElementById(containerId);
		if (!btn || !container) return;

		btn.addEventListener('click', function () {
			var isHidden = container.classList.contains('d-none');
			if (isHidden) {
				container.classList.remove('d-none');
				btn.textContent = 'Hide Degrees Offered';
				btn.setAttribute('aria-expanded', 'true');
			} else {
				container.classList.add('d-none');
				btn.textContent = 'Show Degrees Offered';
				btn.setAttribute('aria-expanded', 'false');
			}
		});
	}

	// Initialize toggles for pages that include these elements
	setupToggle('toggleGraduateBtn', 'graduateDegreeContainer');
	setupToggle('toggleUndergradBtn', 'undergradDegreeContainer');
})();

// Bootstrap interop helper for Blazor components
window.bootstrapInterop = {
	showModal: function (selector) {
		try {
			// jQuery + Bootstrap's modal
			if (window.jQuery) {
				$(selector).modal('show');
			} else {
				console.warn('jQuery not found - cannot show modal via bootstrapInterop');
			}
		} catch (e) {
			console.error('Error showing modal', e);
		}
	},
	hideModal: function (selector) {
		try {
			if (window.jQuery) {
				$(selector).modal('hide');
			}
		} catch (e) {
			console.error('Error hiding modal', e);
		}
	}
};