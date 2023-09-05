import { Component } from '@angular/core';
import { Pagination } from '../_models/pagination';
import { Message } from '../_models/message';
import { MessageService } from '../_services/message.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent {
messages?: Message[];
pagination?: Pagination;
container = 'Unread';
pageNumber = 1;
pageSize = 5;
loading = false;
toastr: any;

constructor(private messageService: MessageService, private toastrService: ToastrService) {}

ngOnInit(): void{
  this.loadMessages();
}

loadMessages() {
  this.loading = true;
  this.messageService.getMessages(this.pageNumber,this.pageSize, this.container).subscribe({
    next: response => {
      this.messages = response.result;
      this.pagination = response.pagination;
      this.loading = false;
    }
  })
}

deleteMessage(id: number) {
  this.messageService.deleteMessage(id).subscribe({
    next: () => {this.messages?.splice(this.messages.findIndex(m => m.id === id),1);
                this.toastr.success('You have deleted message successfully');}
  })
}

pageChanged(event: any) {
if(this.pageNumber !== event.page) {
  this.pageNumber = event.page;
  this.loadMessages();
}
}

}
