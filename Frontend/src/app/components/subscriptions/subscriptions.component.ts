import { Component, OnDestroy, OnInit } from '@angular/core';
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
export class SubscriptionsComponent implements OnInit, OnDestroy {
	loading = true;
	error: string | null = null;
	videos: Video[] = [];
	private sub?: Subscription;

	constructor(private videoService: VideoService) {}

	ngOnInit(): void {
		this.sub = this.videoService.getSubscribedFeed({ page: 1, pageSize: 20 }).subscribe({
			next: (res: PagedResult<Video>) => {
				this.videos = res.items;
				this.loading = false;
			},
			error: (err) => {
				this.error = 'Failed to load subscriptions';
				this.loading = false;
			}
		});
	}

	ngOnDestroy(): void {
		this.sub?.unsubscribe();
	}
}


