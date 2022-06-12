import { Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root'
})
export class BusyService {
  busyRequestsCount = 0; 
  constructor(private spinnerService: NgxSpinnerService) { }

  busy(){
    this.busyRequestsCount++;
    this.spinnerService.show(undefined, {
      type: 'line-scale-party',
      bdColor: 'rgba(0, 0, 0, 0.8)',
      color: '#fff',
      size: 'medium'
    });
  }

  idle() {
    this.busyRequestsCount--;
    if(this.busyRequestsCount <= 0)
    {
      this.busyRequestsCount = 0;
      this.spinnerService.hide();
    }
  }
}
