import { Component, OnDestroy, OnInit, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';
import { VideoService, PagedResult } from '../../services/video.service';
import { Video } from '../../models/video.model';
import { Subscription } from 'rxjs';

@Component({
	selector: 'app-favorites',
	standalone: true,
	imports: [CommonModule, MatIconModule, MatProgressSpinnerModule, MatButtonModule],
	templateUrl: './favorites.component.html',
	styleUrls: ['./favorites.component.scss']
})
export class FavoritesComponent implements OnInit, OnDestroy, AfterViewInit {
	loading = true;
	loadingMore = false;
	error: string | null = null;
	videos: Video[] = [];
	page = 1;
	pageSize = 20;
	hasNextPage = false;
	private sub?: Subscription;
	@ViewChild('loadMoreTrigger', { static: false }) loadMoreTrigger?: ElementRef;
	private observer?: IntersectionObserver;

	constructor(private videoService: VideoService, private router: Router) {}

	ngOnInit(): void {
		this.loadPage(1);
	}

	ngAfterViewInit(): void {
		this.setupIntersectionObserver();
	}

	loadPage(nextPage: number) {
		if (nextPage === 1) this.loading = true; else this.loadingMore = true;
		console.log('Loading favorites page:', nextPage);
		this.sub = this.videoService.getFavoriteVideos({ page: nextPage, pageSize: this.pageSize }).subscribe({
			next: (res: PagedResult<Video>) => {
				console.log('Favorites loaded successfully:', res);
				console.log('Videos:', res.items);
				this.page = res.page;
				this.hasNextPage = res.hasNextPage;
				this.videos = nextPage === 1 ? res.items : [...this.videos, ...res.items];
				this.loading = false;
				this.loadingMore = false;
			},
			error: (err) => {
				console.error('Failed to load favorites:', err);
				console.error('Error details:', {
					status: err.status,
					statusText: err.statusText,
					message: err.message,
					error: err.error
				});
				this.error = 'Failed to load favorites';
				this.loading = false;
				this.loadingMore = false;
			}
		});
	}

	loadMore() {
		if (!this.hasNextPage || this.loadingMore) return;
		this.loadPage(this.page + 1);
	}

	ngOnDestroy(): void {
		this.sub?.unsubscribe();
		this.observer?.disconnect();
	}

	private setupIntersectionObserver(): void {
		if (!this.loadMoreTrigger) return;
		this.observer = new IntersectionObserver((entries) => {
			if (entries[0].isIntersecting && this.hasNextPage && !this.loadingMore) {
				this.loadMore();
			}
		}, { threshold: 0.1 });
		this.observer.observe(this.loadMoreTrigger.nativeElement);
	}

	getSafeImageUrl(imageUrl: string | null | undefined): string {
		// Check if the URL is from a known problematic Azure Storage account
		if (imageUrl && (
			imageUrl.includes('storage.blob.core.windows.net') || 
			imageUrl.includes('drashyamstorage.blob.core.windows.net')
		)) {
			// Return default image for known problematic URLs
			return '/assets/default-video-thumbnail.svg';
		}
		return imageUrl || '/assets/default-video-thumbnail.svg';
	}

	getChannelOrUserName(video: Video): string {
		// First check if channel name is available directly
		if (video.channelName) {
			return video.channelName;
		}
		// Then check if channel object has name
		if (video.channel?.name) {
			return video.channel.name;
		}
		// Then check if user name is available directly
		if (video.userName) {
			return video.userName;
		}
		// Then check if user object has name
		if (video.user?.firstName && video.user?.lastName) {
			return `${video.user.firstName} ${video.user.lastName}`;
		}
		if (video.user?.firstName) {
			return video.user.firstName;
		}
		return 'Unknown Channel';
	}

	onImageError(event: any) {
		// Only log once per image to reduce console noise
		if (!event.target.dataset.fallbackApplied) {
			console.log('Image failed to load, using default image');
			event.target.dataset.fallbackApplied = 'true';
		}
		event.target.src = '/assets/default-video-thumbnail.svg';
	}

	onVideoClick(video: Video) {
		console.log('Navigating to video:', video.id);
		this.router.navigate(['/videos', video.id]);
	}
}


