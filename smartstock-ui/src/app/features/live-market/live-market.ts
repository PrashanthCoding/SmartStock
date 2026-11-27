import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { MarketService, MarketUpdate } from '../../core/services/market.service';

@Component({
  selector: 'app-live-market',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './live-market.html',
  styleUrl: './live-market.css',
})
export class LiveMarket implements OnInit, OnDestroy {
  marketData: MarketUpdate[] = [];

  connectionStatus: string = 'Connecting';

  private subscriptions = new Subscription();

  constructor(private marketService: MarketService) {}

  ngOnInit(): void {
    // Receive market updates
    this.subscriptions.add(
      this.marketService.marketUpdates$.subscribe((update) => {
        const index = this.marketData.findIndex((x) => x.symbol === update.symbol);

        if (index !== -1) {
          this.marketData[index] = update;
        } else {
          this.marketData.push(update);
        }

        this.marketData = [...this.marketData];
      }),
    );

    // Receive connection status
    this.subscriptions.add(
      this.marketService.connectionStatus$.subscribe((status) => {
        this.connectionStatus = status;
      }),
    );

    // Start SignalR connection
    this.marketService.startConnection();
  }

  // Total Trading Pairs
  get totalPairs(): number {
    return this.marketData.length;
  }

  // Positive Movers
  get positiveMovers(): number {
    return this.marketData.filter((item) => item.priceChangePercent > 0).length;
  }

  // Negative Movers
  get negativeMovers(): number {
    return this.marketData.filter((item) => item.priceChangePercent < 0).length;
  }

  // Format Cryptocurrency Price
  formatPrice(price: number): string {
    return price.toLocaleString('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 8,
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();

    // Keep the shared SignalR connection running.
  }
}
