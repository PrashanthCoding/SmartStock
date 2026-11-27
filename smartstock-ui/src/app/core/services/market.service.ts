import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

export interface MarketUpdate {
  symbol: string;
  price: number;
  priceChange: number;
  priceChangePercent: number;
  highPrice: number;
  lowPrice: number;
  volume: number;
  quoteVolume: number;
  lastUpdated: string;
}

@Injectable({
  providedIn: 'root',
})
export class MarketService {
  private hubConnection!: signalR.HubConnection;

  private marketUpdateSubject = new Subject<MarketUpdate>();

  marketUpdates$ = this.marketUpdateSubject.asObservable();

  connectionStatus$ = new Subject<string>();

  startConnection(): void {
    if (this.hubConnection) {
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7101/hubs/market')
      .withAutomaticReconnect()
      .build();

    this.registerMarketEvents();

    this.hubConnection.onreconnecting(() => {
      this.connectionStatus$.next('Reconnecting');
    });

    this.hubConnection.onreconnected(() => {
      this.connectionStatus$.next('Connected');
    });

    this.hubConnection.onclose(() => {
      this.connectionStatus$.next('Disconnected');
    });

    this.hubConnection
      .start()
      .then(() => {
        this.connectionStatus$.next('Connected');
        console.log('Market SignalR Connected');
      })
      .catch((error) => {
        this.connectionStatus$.next('Disconnected');
        console.error('SignalR Connection Error:', error);
      });
  }

  private registerMarketEvents(): void {
    this.hubConnection.on('ReceiveMarketUpdate', (data: MarketUpdate) => {
      this.marketUpdateSubject.next(data);
    });
  }

  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
}
