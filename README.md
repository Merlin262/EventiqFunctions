GET mensagens de um ticket por Id - 
https://ticketfunction.azurewebsites.net/api/ticket/get?ticketId=66e3499de00df66a0a902b5f

POST Cria ticket - 
https://ticketfunction.azurewebsites.net/api/ticket/create?userId=441&adminId=551&title=esta%20na%20Azure&messageContent=Deu%20boa

POST Adiciona mensagem a um Ticket existente - https://ticketfunction.azurewebsites.net/api/ticket/addMessage?ticketId=66e3499de00df66a0a902b5f&senderId=551&messageContent=Mensagem%20foi%20adicionada

PUT Atualiza o titulo e o id do administrador responsável - 
https://ticketfunction.azurewebsites.net/api/ticket/update?ticketId=66e3499de00df66a0a902b5f&title=Atualizando%20titulo&adminId=507

DELETE Deleta Ticket e suas mensagens -
https://ticketfunction.azurewebsites.net/api/ticket/delete?ticketId=66e0e1d4e0a76f07182403a2
