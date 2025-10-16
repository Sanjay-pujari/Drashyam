import { Component, OnDestroy, OnInit, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { VideoService, PagedResult } from '../../services/video.service';
import { Video } from '../../models/video.model';
import { Subscription } from 'rxjs';

@Component({
	selector: 'app-subscriptions',
	standalone: true,
	imports: [CommonModule, MatIconModule, MatProgressSpinnerModule, MatButtonModule],
	templateUrl: './subscriptions.component.html',
	styleUrls: ['./subscriptions.component.scss']
})
export class SubscriptionsComponent implements OnInit, OnDestroy, AfterViewInit {
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

	constructor(private videoService: VideoService) {}

	ngOnInit(): void {
		this.loadPage(1);
	}

	ngAfterViewInit(): void {
		this.setupIntersectionObserver();
	}

	loadPage(nextPage: number) {
		if (nextPage === 1) this.loading = true; else this.loadingMore = true;
		this.sub = this.videoService.getSubscribedFeed({ page: nextPage, pageSize: this.pageSize }).subscribe({
			next: (res: PagedResult<Video>) => {
				this.page = res.page;
				this.hasNextPage = res.hasNextPage;
				this.videos = nextPage === 1 ? res.items : [...this.videos, ...res.items];
				this.loading = false;
				this.loadingMore = false;
			},
			error: () => {
				this.error = 'Failed to load subscriptions';
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
}


