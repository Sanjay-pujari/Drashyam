import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
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
export class FavoritesComponent implements OnInit, OnDestroy {
	loading = true;
	loadingMore = false;
	error: string | null = null;
	videos: Video[] = [];
	page = 1;
	pageSize = 20;
	hasNextPage = false;
	private sub?: Subscription;

	constructor(private videoService: VideoService) {}

	ngOnInit(): void {
		this.loadPage(1);
	}

	loadPage(nextPage: number) {
		if (nextPage === 1) this.loading = true; else this.loadingMore = true;
		this.sub = this.videoService.getFavoriteVideos({ page: nextPage, pageSize: this.pageSize }).subscribe({
			next: (res: PagedResult<Video>) => {
				this.page = res.page;
				this.hasNextPage = res.hasNextPage;
				this.videos = nextPage === 1 ? res.items : [...this.videos, ...res.items];
				this.loading = false;
				this.loadingMore = false;
			},
			error: () => {
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
	}
}


